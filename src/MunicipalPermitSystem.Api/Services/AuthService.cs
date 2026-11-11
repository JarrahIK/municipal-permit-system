using Microsoft.Extensions.Options;
using MunicipalPermitSystem.Api.Configuration;
using MunicipalPermitSystem.Api.DTOs;
using MunicipalPermitSystem.Api.Models;
using MunicipalPermitSystem.Api.Models.Entities;

namespace MunicipalPermitSystem.Api.Services;

public sealed class AuthService : IAuthService
{
    private readonly IAccountStore _accounts;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly SecuritySettings _security;
    private readonly TimeProvider _clock;

    public AuthService(
        IAccountStore accounts,
        IPasswordHasher hasher,
        IJwtTokenService jwt,
        IOptions<SecuritySettings> security,
        TimeProvider clock)
    {
        _accounts = accounts;
        _hasher = hasher;
        _jwt = jwt;
        _security = security.Value;
        _clock = clock;
    }

    public async Task<AuthResult> RegisterCitizenAsync(CitizenRegisterDto dto, CancellationToken ct = default)
    {
        var nationalId = dto.NationalId.Trim();

        if (await _accounts.CitizenExistsAsync(nationalId, ct))
            return new AuthResult(AuthStatus.DuplicateUser);

        var citizen = new Citizen
        {
            NationalId = nationalId,
            FullName = dto.FullName.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Address = dto.Address.Trim(),
            PasswordHash = _hasher.Hash(dto.Password),
            CreatedAt = _clock.GetUtcNow()
        };

        if (!await _accounts.TryAddCitizenAsync(citizen, ct))
            return new AuthResult(AuthStatus.DuplicateUser);

        return Success(citizen.NationalId, citizen.FullName, AppRoles.Citizen);
    }

    public async Task<AuthResult> LoginCitizenAsync(CitizenLoginDto dto, CancellationToken ct = default)
    {
        var citizen = await _accounts.FindCitizenAsync(dto.NationalId.Trim(), ct);

        var status = await CheckPasswordAsync(citizen, dto.Password, ct);
        return status == AuthStatus.Success && citizen is not null
            ? Success(citizen.NationalId, citizen.FullName, AppRoles.Citizen)
            : new AuthResult(status);
    }

    public async Task<AuthResult> LoginEmployeeAsync(EmployeeLoginDto dto, CancellationToken ct = default)
    {
        var employee = await _accounts.FindEmployeeAsync(dto.EmployeeId.Trim(), ct);

        var status = await CheckPasswordAsync(employee, dto.Password, ct);
        return status == AuthStatus.Success && employee is not null
            ? Success(employee.EmployeeId, employee.FullName, employee.Role.ToString())
            : new AuthResult(status);
    }

    /// <summary>
    /// NFR1.1: after N failed attempts the account is locked for M minutes.
    /// Returns Success, InvalidCredentials or AccountLocked.
    /// </summary>
    private async Task<AuthStatus> CheckPasswordAsync(ILockableAccount? account, string password, CancellationToken ct)
    {
        if (account is null)
        {
            _hasher.SpendTimeLikeAVerify(password); // unknown ID takes as long as a wrong password
            return AuthStatus.InvalidCredentials;
        }

        var now = _clock.GetUtcNow();

        if (account.LockoutEnd is { } lockoutEnd)
        {
            if (lockoutEnd > now) return AuthStatus.AccountLocked;

            // The lockout has expired: start fresh instead of re-locking on the very next typo.
            account.FailedLoginAttempts = 0;
            account.LockoutEnd = null;
        }

        if (!_hasher.Verify(password, account.PasswordHash))
        {
            account.FailedLoginAttempts++;
            if (account.FailedLoginAttempts >= _security.MaxFailedLoginAttempts)
                account.LockoutEnd = now.AddMinutes(_security.LockoutMinutes);

            await _accounts.SaveChangesAsync(ct);
            return AuthStatus.InvalidCredentials;
        }

        account.FailedLoginAttempts = 0;
        account.LockoutEnd = null;
        await _accounts.SaveChangesAsync(ct);
        return AuthStatus.Success;
    }

    private AuthResult Success(string subjectId, string name, string role)
    {
        var (token, expiresAt) = _jwt.CreateToken(subjectId, name, role);
        return new AuthResult(AuthStatus.Success, new TokenResponseDto
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            Name = name,
            Role = role
        });
    }
}

using MunicipalPermitSystem.Api.DTOs;

namespace MunicipalPermitSystem.Api.Services;

public enum AuthStatus
{
    Success,
    InvalidCredentials,
    AccountLocked,
    DuplicateUser
}

public sealed record AuthResult(AuthStatus Status, TokenResponseDto? Token = null);

public interface IAuthService
{
    Task<AuthResult> RegisterCitizenAsync(CitizenRegisterDto dto, CancellationToken ct = default);
    Task<AuthResult> LoginCitizenAsync(CitizenLoginDto dto, CancellationToken ct = default);
    Task<AuthResult> LoginEmployeeAsync(EmployeeLoginDto dto, CancellationToken ct = default);
}

using Xunit;
using Microsoft.Extensions.Options;
using MunicipalPermitSystem.Api.Configuration;
using MunicipalPermitSystem.Api.DTOs;
using MunicipalPermitSystem.Api.Models.Entities;
using MunicipalPermitSystem.Api.Models.Enums;
using MunicipalPermitSystem.Api.Services;
using MunicipalPermitSystem.Tests.Fakes;

namespace MunicipalPermitSystem.Tests;

public class AuthServiceTests
{
    private const string NationalId = "9991234567";
    private const string Password = "CitizenPass123!";

    private readonly FakeAccountStore _store = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeClock _clock = new();

    private AuthService MakeService(SecuritySettings? settings = null) =>
        new(_store, _hasher, new FakeJwtTokenService(),
            Options.Create(settings ?? new SecuritySettings()), _clock);

    private static CitizenRegisterDto NewCitizen() => new()
    {
        NationalId = NationalId,
        FullName = "Test Citizen",
        Password = Password,
        PhoneNumber = "0791234567",
        Address = "Irbid, Jordan"
    };

    private static CitizenLoginDto CitizenLogin(string password) => new() { NationalId = NationalId, Password = password };

    private async Task<AuthService> ServiceWithRegisteredCitizen(SecuritySettings? settings = null)
    {
        var service = MakeService(settings);
        await service.RegisterCitizenAsync(NewCitizen());
        return service;
    }

    private async Task FailLoginTimes(AuthService service, int times)
    {
        for (var i = 0; i < times; i++)
            Assert.Equal(AuthStatus.InvalidCredentials, (await service.LoginCitizenAsync(CitizenLogin("wrong"))).Status);
    }

    // ---------- registration ----------

    [Fact]
    public async Task Register_stores_a_hashed_password_and_returns_a_citizen_token()
    {
        var result = await MakeService().RegisterCitizenAsync(NewCitizen());

        Assert.Equal(AuthStatus.Success, result.Status);
        Assert.Equal("Citizen", result.Token!.Role);
        Assert.Equal("token:9991234567:Citizen", result.Token.AccessToken);
        Assert.NotEqual(Password, _store.Citizens[NationalId].PasswordHash);
    }

    [Fact]
    public async Task Register_twice_with_the_same_national_id_is_a_duplicate()
    {
        var service = MakeService();
        await service.RegisterCitizenAsync(NewCitizen());

        Assert.Equal(AuthStatus.DuplicateUser, (await service.RegisterCitizenAsync(NewCitizen())).Status);
    }

    // ---------- citizen login ----------

    [Fact]
    public async Task Login_with_the_right_password_succeeds()
    {
        var service = await ServiceWithRegisteredCitizen();

        var result = await service.LoginCitizenAsync(CitizenLogin(Password));

        Assert.Equal(AuthStatus.Success, result.Status);
        Assert.Equal("Citizen", result.Token!.Role);
    }

    [Fact]
    public async Task Login_with_a_wrong_password_is_invalid_credentials()
    {
        var service = await ServiceWithRegisteredCitizen();

        Assert.Equal(AuthStatus.InvalidCredentials, (await service.LoginCitizenAsync(CitizenLogin("nope"))).Status);
    }

    [Fact]
    public async Task Unknown_id_is_invalid_credentials_and_still_spends_hashing_time()
    {
        var result = await MakeService().LoginCitizenAsync(new CitizenLoginDto { NationalId = "0000", Password = "whatever1" });

        Assert.Equal(AuthStatus.InvalidCredentials, result.Status);
        Assert.Equal(1, _hasher.DummyVerifyCalls); // same effort as a real check, so IDs can't be guessed by timing
    }

    // ---------- NFR1.1 lockout ----------

    [Fact]
    public async Task Four_failures_do_not_lock_the_account()
    {
        var service = await ServiceWithRegisteredCitizen();
        await FailLoginTimes(service, 4);

        Assert.Equal(AuthStatus.Success, (await service.LoginCitizenAsync(CitizenLogin(Password))).Status);
    }

    [Fact]
    public async Task Five_failures_lock_the_account_even_for_the_correct_password()
    {
        var service = await ServiceWithRegisteredCitizen();
        await FailLoginTimes(service, 5);

        Assert.Equal(AuthStatus.AccountLocked, (await service.LoginCitizenAsync(CitizenLogin(Password))).Status);
    }

    [Fact]
    public async Task A_locked_account_does_not_even_check_the_password()
    {
        var service = await ServiceWithRegisteredCitizen();
        await FailLoginTimes(service, 5);
        var verifiesBefore = _hasher.VerifyCalls;

        await service.LoginCitizenAsync(CitizenLogin(Password));

        Assert.Equal(verifiesBefore, _hasher.VerifyCalls);
    }

    [Fact]
    public async Task Lockout_lasts_fifteen_minutes_then_login_works_again()
    {
        var service = await ServiceWithRegisteredCitizen();
        await FailLoginTimes(service, 5);

        _clock.Advance(TimeSpan.FromMinutes(14));
        Assert.Equal(AuthStatus.AccountLocked, (await service.LoginCitizenAsync(CitizenLogin(Password))).Status);

        _clock.Advance(TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1));
        Assert.Equal(AuthStatus.Success, (await service.LoginCitizenAsync(CitizenLogin(Password))).Status);

        var citizen = _store.Citizens[NationalId];
        Assert.Equal(0, citizen.FailedLoginAttempts);
        Assert.Null(citizen.LockoutEnd);
    }

    [Fact]
    public async Task One_typo_after_an_expired_lockout_does_not_lock_again()
    {
        var service = await ServiceWithRegisteredCitizen();
        await FailLoginTimes(service, 5);
        _clock.Advance(TimeSpan.FromMinutes(16));

        await FailLoginTimes(service, 1);

        Assert.Equal(AuthStatus.Success, (await service.LoginCitizenAsync(CitizenLogin(Password))).Status);
    }

    [Fact]
    public async Task A_successful_login_resets_the_failure_counter()
    {
        var service = await ServiceWithRegisteredCitizen();
        await FailLoginTimes(service, 4);
        await service.LoginCitizenAsync(CitizenLogin(Password));

        await FailLoginTimes(service, 4); // would be 8 in total without the reset

        Assert.Equal(AuthStatus.Success, (await service.LoginCitizenAsync(CitizenLogin(Password))).Status);
    }

    [Fact]
    public async Task Failed_attempts_are_saved()
    {
        var service = await ServiceWithRegisteredCitizen();
        var savesBefore = _store.SaveCount;

        await FailLoginTimes(service, 2);

        Assert.Equal(savesBefore + 2, _store.SaveCount);
        Assert.Equal(2, _store.Citizens[NationalId].FailedLoginAttempts);
    }

    [Fact]
    public async Task Lockout_limits_come_from_settings()
    {
        var service = await ServiceWithRegisteredCitizen(
            new SecuritySettings { MaxFailedLoginAttempts = 3, LockoutMinutes = 1 });

        await FailLoginTimes(service, 3);
        Assert.Equal(AuthStatus.AccountLocked, (await service.LoginCitizenAsync(CitizenLogin(Password))).Status);

        _clock.Advance(TimeSpan.FromSeconds(61));
        Assert.Equal(AuthStatus.Success, (await service.LoginCitizenAsync(CitizenLogin(Password))).Status);
    }

    // ---------- staff login ----------

    private void AddEmployee(string id, EmployeeRole role) =>
        _store.Employees[id] = new Employee
        {
            EmployeeId = id,
            FullName = "Test " + role,
            Role = role,
            Department = "Building Permits",
            PasswordHash = _hasher.Hash("StaffPass123!")
        };

    [Fact]
    public async Task Every_staff_role_logs_in_with_its_own_role()
    {
        var service = MakeService();
        foreach (var role in Enum.GetValues<EmployeeRole>())
        {
            AddEmployee("E-" + role, role);

            var result = await service.LoginEmployeeAsync(new EmployeeLoginDto { EmployeeId = "E-" + role, Password = "StaffPass123!" });

            Assert.Equal(AuthStatus.Success, result.Status);
            Assert.Equal(role.ToString(), result.Token!.Role);
        }
    }

    [Fact]
    public async Task Citizen_and_staff_logins_are_separate()
    {
        var service = await ServiceWithRegisteredCitizen();
        AddEmployee("SURV001", EmployeeRole.Surveyor);

        var citizenOnStaffLogin = await service.LoginEmployeeAsync(new EmployeeLoginDto { EmployeeId = NationalId, Password = Password });
        var staffOnCitizenLogin = await service.LoginCitizenAsync(new CitizenLoginDto { NationalId = "SURV001", Password = "StaffPass123!" });

        Assert.Equal(AuthStatus.InvalidCredentials, citizenOnStaffLogin.Status);
        Assert.Equal(AuthStatus.InvalidCredentials, staffOnCitizenLogin.Status);
    }

    [Fact]
    public async Task Staff_accounts_lock_too()
    {
        var service = MakeService();
        AddEmployee("MGR001", EmployeeRole.Manager);

        for (var i = 0; i < 5; i++)
            await service.LoginEmployeeAsync(new EmployeeLoginDto { EmployeeId = "MGR001", Password = "bad" });

        var result = await service.LoginEmployeeAsync(new EmployeeLoginDto { EmployeeId = "MGR001", Password = "StaffPass123!" });
        Assert.Equal(AuthStatus.AccountLocked, result.Status);
    }
}

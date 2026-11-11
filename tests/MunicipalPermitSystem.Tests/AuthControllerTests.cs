using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MunicipalPermitSystem.Api.Controllers;
using MunicipalPermitSystem.Api.DTOs;
using MunicipalPermitSystem.Api.Services;

namespace MunicipalPermitSystem.Tests;

/// <summary>Checks that each outcome of the login logic becomes the right HTTP status code.</summary>
public class AuthControllerTests
{
    private sealed class StubAuth(AuthStatus status) : IAuthService
    {
        private AuthResult Result() => new(status, status == AuthStatus.Success
            ? new TokenResponseDto { AccessToken = "t", Role = "Citizen" }
            : null);

        public Task<AuthResult> RegisterCitizenAsync(CitizenRegisterDto dto, CancellationToken ct = default) => Task.FromResult(Result());
        public Task<AuthResult> LoginCitizenAsync(CitizenLoginDto dto, CancellationToken ct = default) => Task.FromResult(Result());
        public Task<AuthResult> LoginEmployeeAsync(EmployeeLoginDto dto, CancellationToken ct = default) => Task.FromResult(Result());
    }

    private static AuthController Make(AuthStatus status)
    {
        var services = new ServiceCollection().AddLogging().AddControllers().Services.BuildServiceProvider();
        return new AuthController(new StubAuth(status))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services } }
        };
    }

    private static int? Code(IActionResult? result) =>
        (result as ObjectResult)?.StatusCode ?? (result as StatusCodeResult)?.StatusCode;

    [Fact]
    public async Task Register_success_is_201() =>
        Assert.Equal(201, Code((await Make(AuthStatus.Success).RegisterCitizen(new CitizenRegisterDto(), default)).Result));

    [Fact]
    public async Task Register_duplicate_is_409() =>
        Assert.Equal(409, Code((await Make(AuthStatus.DuplicateUser).RegisterCitizen(new CitizenRegisterDto(), default)).Result));

    [Fact]
    public async Task Citizen_login_maps_success_wrong_password_and_lockout()
    {
        Assert.Equal(200, Code((await Make(AuthStatus.Success).LoginCitizen(new CitizenLoginDto(), default)).Result));
        Assert.Equal(401, Code((await Make(AuthStatus.InvalidCredentials).LoginCitizen(new CitizenLoginDto(), default)).Result));
        Assert.Equal(423, Code((await Make(AuthStatus.AccountLocked).LoginCitizen(new CitizenLoginDto(), default)).Result));
    }

    [Fact]
    public async Task Staff_login_maps_success_wrong_password_and_lockout()
    {
        Assert.Equal(200, Code((await Make(AuthStatus.Success).LoginStaff(new EmployeeLoginDto(), default)).Result));
        Assert.Equal(401, Code((await Make(AuthStatus.InvalidCredentials).LoginStaff(new EmployeeLoginDto(), default)).Result));
        Assert.Equal(423, Code((await Make(AuthStatus.AccountLocked).LoginStaff(new EmployeeLoginDto(), default)).Result));
    }

    [Fact]
    public void Forgot_password_is_501_until_it_is_built() =>
        Assert.Equal(501, Code(Make(AuthStatus.Success).ForgotPassword(new ForgotPasswordDto())));
}

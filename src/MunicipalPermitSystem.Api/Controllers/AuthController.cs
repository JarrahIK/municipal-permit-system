using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MunicipalPermitSystem.Api.DTOs;
using MunicipalPermitSystem.Api.Services;

namespace MunicipalPermitSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>FR1.2 — register a citizen account.</summary>
    [HttpPost("citizen/register")]
    [ProducesResponseType<TokenResponseDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TokenResponseDto>> RegisterCitizen(CitizenRegisterDto dto, CancellationToken ct)
    {
        var result = await _auth.RegisterCitizenAsync(dto, ct);
        return result.Status switch
        {
            AuthStatus.Success => StatusCode(StatusCodes.Status201Created, result.Token),
            AuthStatus.DuplicateUser => Problem(
                detail: "A citizen with this National ID is already registered.",
                statusCode: StatusCodes.Status409Conflict),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>FR1.3 — citizen login.</summary>
    [HttpPost("citizen/login")]
    public async Task<ActionResult<TokenResponseDto>> LoginCitizen(CitizenLoginDto dto, CancellationToken ct)
        => ToActionResult(await _auth.LoginCitizenAsync(dto, ct));

    /// <summary>Staff login (Surveyor / Department Head / Manager / Accountant) — separate from citizens.</summary>
    [HttpPost("staff/login")]
    public async Task<ActionResult<TokenResponseDto>> LoginStaff(EmployeeLoginDto dto, CancellationToken ct)
        => ToActionResult(await _auth.LoginEmployeeAsync(dto, ct));

    /// <summary>FR1.4 — not built yet: needs a decision on the recovery channel (email vs SMS).</summary>
    [HttpPost("citizen/forgot-password")]
    public IActionResult ForgotPassword(ForgotPasswordDto dto)
        => Problem(
            detail: "Not implemented yet — pending a decision on the recovery method (FR1.4).",
            statusCode: StatusCodes.Status501NotImplemented);

    /// <summary>Who does my token say I am? Handy for checking logins and roles.</summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<CurrentUserDto> Me()
        => new CurrentUserDto
        {
            Subject = User.FindFirst("sub")?.Value ?? string.Empty,
            Name = User.FindFirst("name")?.Value ?? string.Empty,
            Role = User.FindFirst("role")?.Value ?? string.Empty
        };

    private ActionResult<TokenResponseDto> ToActionResult(AuthResult result) => result.Status switch
    {
        AuthStatus.Success => Ok(result.Token),
        AuthStatus.AccountLocked => Problem(
            detail: "Account temporarily locked after too many failed attempts.",
            statusCode: StatusCodes.Status423Locked),
        _ => Problem(
            detail: "Invalid ID or password.",
            statusCode: StatusCodes.Status401Unauthorized)
    };
}

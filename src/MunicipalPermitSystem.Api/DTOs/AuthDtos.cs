using System.ComponentModel.DataAnnotations;

namespace MunicipalPermitSystem.Api.DTOs;

/// <summary>FR1.2 — register with National ID.</summary>
public class CitizenRegisterDto
{
    [Required, StringLength(20, MinimumLength = 1)]
    public string NationalId { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;

    // bcrypt only uses the first 72 bytes of a password, so longer ones are refused.
    [Required, StringLength(72, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 1)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(255, MinimumLength = 1)]
    public string Address { get; set; } = string.Empty;
}

/// <summary>FR1.3 — log in with National ID + password.</summary>
public class CitizenLoginDto
{
    [Required]
    public string NationalId { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Separate staff login (Surveyor / Dept Head / Manager / Accountant).</summary>
public class EmployeeLoginDto
{
    [Required]
    public string EmployeeId { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    [Required]
    public string NationalId { get; set; } = string.Empty;
}

public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTimeOffset ExpiresAt { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>"Citizen" | "Surveyor" | "DepartmentHead" | "Manager" | "Accountant"</summary>
    public string Role { get; set; } = string.Empty;
}

public class CurrentUserDto
{
    public string Subject { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

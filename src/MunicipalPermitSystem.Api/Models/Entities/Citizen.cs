using System.ComponentModel.DataAnnotations;

namespace MunicipalPermitSystem.Api.Models.Entities;

/// <summary>CITIZEN table in the ER diagram.</summary>
public class Citizen : ILockableAccount
{
    [Key]
    [MaxLength(20)]
    public string NationalId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    // AES-256-GCM encrypted at rest (NFR1.2) — configured in ApplicationDbContext.
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // NFR1.1 lockout bookkeeping
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

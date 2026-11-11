using System.ComponentModel.DataAnnotations;
using MunicipalPermitSystem.Api.Models.Enums;

namespace MunicipalPermitSystem.Api.Models.Entities;

/// <summary>EMPLOYEE table in the ER diagram (Surveyor / DepartmentHead / Manager / Accountant).</summary>
public class Employee : ILockableAccount
{
    [Key]
    [MaxLength(20)]
    public string EmployeeId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    // AES-256-GCM encrypted at rest (NFR1.2) — configured in ApplicationDbContext.
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    public EmployeeRole Role { get; set; }

    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

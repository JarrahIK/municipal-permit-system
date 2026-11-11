using System.ComponentModel.DataAnnotations;
using MunicipalPermitSystem.Api.Models.Enums;

namespace MunicipalPermitSystem.Api.Models.Entities;

/// <summary>
/// Not in the original ER diagram — closes its gap. The ER diagram has no department_head_id /
/// accountant_id on PERMIT_REQUEST. Instead of adding more nullable keys, every review, approval
/// or rejection by ANY role is recorded here. It is also the audit trail required by SFR8.
/// </summary>
public class ApprovalLog
{
    [Key]
    public int LogId { get; set; }

    public int RequestId { get; set; }
    public PermitRequest? PermitRequest { get; set; }

    public ActorType ActorType { get; set; }

    /// <summary>Citizen NationalId or Employee EmployeeId, depending on ActorType.</summary>
    [MaxLength(20)]
    public string ActorId { get; set; } = string.Empty;

    public EmployeeRole? EmployeeRole { get; set; }

    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

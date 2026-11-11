using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MunicipalPermitSystem.Api.Models.Enums;

namespace MunicipalPermitSystem.Api.Models.Entities;

/// <summary>
/// PERMIT_REQUEST in the ER diagram. Abstract: every request is one of the four permit kinds
/// (ResidentialPermit, CommercialPermit, ParkingPermit, SpecialStructurePermit), each stored in
/// its own table sharing this table's key ("table per type") — the same shape as the class
/// diagram and the ER diagram. Querying PermitRequests returns the right subclass automatically.
/// </summary>
public abstract class PermitRequest
{
    protected PermitRequest(PermitType permitType)
    {
        PermitType = permitType;
    }

    [Key]
    public int RequestId { get; set; }

    [MaxLength(20)]
    public string CitizenId { get; set; } = string.Empty;
    public Citizen? Citizen { get; set; }

    // Assigned later in the workflow, hence nullable.
    [MaxLength(20)]
    public string? SurveyorId { get; set; }
    public Employee? Surveyor { get; set; }

    [MaxLength(20)]
    public string? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public PermitType PermitType { get; set; }
    public DateTimeOffset SubmissionDate { get; set; } = DateTimeOffset.UtcNow;
    public PermitStatus Status { get; set; } = PermitStatus.Submitted;

    [Column(TypeName = "decimal(19,2)")]
    public decimal TotalEstimatedFee { get; set; }
    public bool IsViolationPresent { get; set; }

    public List<BuildingComponent> BuildingComponents { get; set; } = new();
    public List<PermitDocument> Documents { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public List<ApprovalLog> ApprovalLogs { get; set; } = new();
}

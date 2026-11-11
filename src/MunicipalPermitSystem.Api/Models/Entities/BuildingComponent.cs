using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MunicipalPermitSystem.Api.Models.Entities;

/// <summary>BUILDING_COMPONENT: basements, floors, staircases, ...</summary>
public class BuildingComponent
{
    [Key]
    public int ComponentId { get; set; }

    public int RequestId { get; set; }
    public PermitRequest? PermitRequest { get; set; }

    [MaxLength(100)]
    public string ComponentType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal AreaM2 { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal LengthM { get; set; }

    public bool IsViolation { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}

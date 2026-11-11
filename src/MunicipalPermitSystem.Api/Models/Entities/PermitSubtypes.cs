using System.ComponentModel.DataAnnotations.Schema;
using MunicipalPermitSystem.Api.Models.Enums;

namespace MunicipalPermitSystem.Api.Models.Entities;

public class ResidentialPermit : PermitRequest
{
    public ResidentialPermit() : base(PermitType.Residential) { }

    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string FloorType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal BalconyArea { get; set; }

    [Column(TypeName = "decimal(19,2)")]
    public decimal FixedResidentialPrice { get; set; }

    public int NumberOfFloors { get; set; }
}

public class CommercialPermit : PermitRequest
{
    public CommercialPermit() : base(PermitType.Commercial) { }

    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string ProjectionType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal ShopArea { get; set; }

    [Column(TypeName = "decimal(19,2)")]
    public decimal FixedCommercialFee { get; set; }

    // In the class diagram but not in the ER diagram — nullable until confirmed.
    public int? NumberOfFloors { get; set; }
}

public class ParkingPermit : PermitRequest
{
    public ParkingPermit() : base(PermitType.Parking) { }

    public int NumberOfParkingSpaces { get; set; }
}

public class SpecialStructurePermit : PermitRequest
{
    public SpecialStructurePermit() : base(PermitType.SpecialStructure) { }

    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string StructureType { get; set; } = string.Empty;
}

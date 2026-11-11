using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MunicipalPermitSystem.Api.Models.Enums;

namespace MunicipalPermitSystem.Api.Models.Entities;

public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int RequestId { get; set; }
    public PermitRequest? PermitRequest { get; set; }

    [Column(TypeName = "decimal(19,2)")]
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTimeOffset? TransactionDate { get; set; }

    // Payment is mocked. On confirmation a hash of the transaction is stored here as the
    // "blockchain" audit entry (Phase 3).
    [MaxLength(128)]
    public string? BlockchainHash { get; set; }
}

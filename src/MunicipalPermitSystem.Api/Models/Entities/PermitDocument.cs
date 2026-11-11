using System.ComponentModel.DataAnnotations;

namespace MunicipalPermitSystem.Api.Models.Entities;

/// <summary>DOCUMENT in the ER diagram (named PermitDocument to avoid generic-name clashes).</summary>
public class PermitDocument
{
    [Key]
    public int DocumentId { get; set; }

    public int RequestId { get; set; }
    public PermitRequest? PermitRequest { get; set; }

    [MaxLength(100)]
    public string DocumentType { get; set; } = string.Empty;

    public DateTimeOffset UploadDate { get; set; } = DateTimeOffset.UtcNow;

    // Practical addition (not in the diagrams): where the uploaded file is stored.
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
}

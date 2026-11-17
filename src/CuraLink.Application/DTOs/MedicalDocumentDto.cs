using CuraLink.Domain.Enums;

namespace CuraLink.Application.DTOs;

public class MedicalDocumentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public VisibilityStatus VisibilityStatus { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}

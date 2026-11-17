using CuraLink.Domain.Enums;

namespace CuraLink.Domain.Entities;

public class MedicalDocument
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public string UploadedByUserId { get; set; } = string.Empty;
    public ApplicationUser UploadedByUser { get; set; } = null!;
    public int? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public DocumentType DocumentType { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public VisibilityStatus VisibilityStatus { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}

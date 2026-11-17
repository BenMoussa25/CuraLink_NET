namespace CuraLink.Domain.Entities;

public class MedicalCondition
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime RecordedAtUtc { get; set; }
    public int? RecordedByDoctorId { get; set; }
    public Doctor? RecordedByDoctor { get; set; }
}

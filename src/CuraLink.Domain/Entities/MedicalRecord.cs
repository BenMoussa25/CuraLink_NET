namespace CuraLink.Domain.Entities;

public class MedicalRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
    public ICollection<MedicalCondition> Conditions { get; set; } = new List<MedicalCondition>();
}

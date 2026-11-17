namespace CuraLink.Domain.Entities;

public class Allergy
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public string? Notes { get; set; }
}

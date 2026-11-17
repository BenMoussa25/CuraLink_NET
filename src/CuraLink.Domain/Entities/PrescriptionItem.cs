namespace CuraLink.Domain.Entities;

public class PrescriptionItem
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;
    public string MedicineName { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string DosageInstructions { get; set; } = string.Empty;
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public string? Route { get; set; }
    public string? AdditionalInstructions { get; set; }
}

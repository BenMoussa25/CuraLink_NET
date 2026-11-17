using CuraLink.Domain.Enums;

namespace CuraLink.Domain.Entities;

public class Prescription
{
    public int Id { get; set; }
    public int ConsultationId { get; set; }
    public Consultation Consultation { get; set; } = null!;
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public DateTime IssuedAtUtc { get; set; }
    public string? GeneralInstructions { get; set; }
    public PrescriptionStatus Status { get; set; }
    public bool IsSharedWithPatient { get; set; }
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}

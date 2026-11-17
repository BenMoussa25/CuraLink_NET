namespace CuraLink.Domain.Entities;

public class Patient
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<MedicalDocument> Documents { get; set; } = new List<MedicalDocument>();
    public ICollection<PatientNote> Notes { get; set; } = new List<PatientNote>();
}

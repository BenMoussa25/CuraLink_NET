using CuraLink.Domain.Enums;

namespace CuraLink.Domain.Entities;

public class Consultation
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public string? ReportedSymptoms { get; set; }
    public string? ClinicalObservations { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Recommendations { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public ConsultationStatus Status { get; set; }
    public bool IsSharedWithPatient { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? FinalizedAtUtc { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

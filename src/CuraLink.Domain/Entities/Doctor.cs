using CuraLink.Domain.Enums;

namespace CuraLink.Domain.Entities;

public class Doctor
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;
    public string Biography { get; set; } = string.Empty;
    public string? ClinicName { get; set; }
    public string? ClinicAddress { get; set; }
    public string? City { get; set; }
    public string? PublicPhone { get; set; }
    public decimal? ConsultationFee { get; set; }
    public string? CurrencyCode { get; set; }
    public DoctorVerificationStatus VerificationStatus { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? ProfileImagePath { get; set; }
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    public ICollection<ScheduleException> Exceptions { get; set; } = new List<ScheduleException>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<MedicalDocument> Documents { get; set; } = new List<MedicalDocument>();
}

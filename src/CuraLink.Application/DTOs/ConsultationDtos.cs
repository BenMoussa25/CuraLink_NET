using System.ComponentModel.DataAnnotations;
using CuraLink.Domain.Enums;

namespace CuraLink.Application.DTOs;

public class ConsultationDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateTime ConsultationDate { get; set; }
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
}

public class ConsultationInput
{
    [Range(1, int.MaxValue)]
    public int AppointmentId { get; set; }

    [Required]
    [StringLength(300)]
    public string Reason { get; set; } = string.Empty;

    public string? ReportedSymptoms { get; set; }
    public string? ClinicalObservations { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Recommendations { get; set; }
    public DateTime? FollowUpDate { get; set; }
}

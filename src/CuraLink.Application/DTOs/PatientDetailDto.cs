namespace CuraLink.Application.DTOs;

public class PatientDetailDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public IReadOnlyList<AllergyDto> Allergies { get; set; } = new List<AllergyDto>();
    public IReadOnlyList<MedicalConditionDto> Conditions { get; set; } = new List<MedicalConditionDto>();
    public IReadOnlyList<ConsultationDto> Consultations { get; set; } = new List<ConsultationDto>();
    public IReadOnlyList<PrescriptionDto> Prescriptions { get; set; } = new List<PrescriptionDto>();
    public IReadOnlyList<MedicalDocumentDto> Documents { get; set; } = new List<MedicalDocumentDto>();
}

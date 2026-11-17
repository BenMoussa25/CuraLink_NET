namespace CuraLink.Application.DTOs;

public class PatientSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? City { get; set; }
    public int AppointmentCount { get; set; }
    public DateTime? LastAppointmentUtc { get; set; }
}

public class AllergyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public string? Notes { get; set; }
}

public class MedicalConditionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}

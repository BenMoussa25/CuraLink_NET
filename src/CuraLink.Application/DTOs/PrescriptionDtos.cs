using System.ComponentModel.DataAnnotations;
using CuraLink.Domain.Enums;

namespace CuraLink.Application.DTOs;

public class PrescriptionDto
{
    public int Id { get; set; }
    public int ConsultationId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public string? GeneralInstructions { get; set; }
    public PrescriptionStatus Status { get; set; }
    public bool IsSharedWithPatient { get; set; }
    public IReadOnlyList<PrescriptionItemDto> Items { get; set; } = new List<PrescriptionItemDto>();
}

public class PrescriptionItemDto
{
    public int Id { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? DosageInstructions { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public string? Route { get; set; }
    public string? AdditionalInstructions { get; set; }
}

public class PrescriptionInput
{
    [Range(1, int.MaxValue)]
    public int ConsultationId { get; set; }

    [StringLength(1000)]
    public string? GeneralInstructions { get; set; }

    [MinLength(1)]
    public List<PrescriptionItemInput> Items { get; set; } = new List<PrescriptionItemInput>();
}

public class PrescriptionItemInput
{
    [Required]
    [StringLength(200)]
    public string MedicineName { get; set; } = string.Empty;

    public string? Strength { get; set; }

    [Required]
    [StringLength(500)]
    public string DosageInstructions { get; set; } = string.Empty;

    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public string? Route { get; set; }
    public string? AdditionalInstructions { get; set; }
}

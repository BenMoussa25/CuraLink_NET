using System.ComponentModel.DataAnnotations;
using CuraLink.Domain.Enums;

namespace CuraLink.Application.DTOs;

public class DoctorProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
    public string? ClinicName { get; set; }
    public string? ClinicAddress { get; set; }
    public string? City { get; set; }
    public string? PublicPhone { get; set; }
    public decimal? ConsultationFee { get; set; }
    public string? CurrencyCode { get; set; }
    public int SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = string.Empty;
    public DoctorVerificationStatus VerificationStatus { get; set; }
    public string? ProfileImagePath { get; set; }
}

public class DoctorProfileInput
{
    [Required]
    [StringLength(2000)]
    public string Biography { get; set; } = string.Empty;

    [StringLength(150)]
    public string? ClinicName { get; set; }

    [StringLength(300)]
    public string? ClinicAddress { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(30)]
    public string? PublicPhone { get; set; }

    [Range(0, 100000)]
    public decimal? ConsultationFee { get; set; }

    [StringLength(8)]
    public string? CurrencyCode { get; set; }

    [Range(1, int.MaxValue)]
    public int SpecialtyId { get; set; }

    [StringLength(300)]
    public string? ProfileImagePath { get; set; }
}

public class SpecialtyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

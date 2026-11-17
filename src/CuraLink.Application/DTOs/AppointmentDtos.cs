using System.ComponentModel.DataAnnotations;
using CuraLink.Domain.Enums;

namespace CuraLink.Application.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? PatientReason { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class AppointmentFilter
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public AppointmentStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AppointmentRequestDto
{
    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }

    public DateTime StartAtUtc { get; set; }

    [StringLength(500)]
    public string? PatientReason { get; set; }
}

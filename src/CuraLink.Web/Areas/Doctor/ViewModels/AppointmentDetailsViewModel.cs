using CuraLink.Application.DTOs;
using CuraLink.Domain.Enums;

namespace CuraLink.Web.Areas.Doctor.ViewModels;

public class AppointmentDetailsViewModel
{
    public AppointmentDto Appointment { get; set; } = null!;

    public AppointmentStatus NewStatus { get; set; }

    public string? Reason { get; set; }

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }
}

using CuraLink.Application.DTOs;

namespace CuraLink.Web.Areas.Doctor.ViewModels;

public class ConsultationFormViewModel
{
    public ConsultationInput Input { get; set; } = new();

    public IReadOnlyList<AppointmentDto> Appointments { get; set; } = new List<AppointmentDto>();

    public int ConsultationId { get; set; }
}

using CuraLink.Application.Common;
using CuraLink.Application.DTOs;

namespace CuraLink.Web.Areas.Doctor.ViewModels;

public class AppointmentIndexViewModel
{
    public AppointmentFilter Filter { get; set; } = new();

    public PagedResult<AppointmentDto> Result { get; set; } = new();
}

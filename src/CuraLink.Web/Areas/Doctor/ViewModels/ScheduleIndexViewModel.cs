using CuraLink.Application.DTOs;

namespace CuraLink.Web.Areas.Doctor.ViewModels;

public class ScheduleIndexViewModel
{
    public IReadOnlyList<ScheduleDto> Schedules { get; set; } = new List<ScheduleDto>();

    public IReadOnlyList<ScheduleExceptionDto> Exceptions { get; set; } = new List<ScheduleExceptionDto>();
}

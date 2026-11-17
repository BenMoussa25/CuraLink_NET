using CuraLink.Application.DTOs;

namespace CuraLink.Web.Areas.Doctor.ViewModels;

public class ScheduleExceptionsViewModel
{
    public IReadOnlyList<ScheduleExceptionDto> Exceptions { get; set; } = new List<ScheduleExceptionDto>();

    public ScheduleExceptionInput Input { get; set; } = new();
}

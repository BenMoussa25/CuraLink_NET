using CuraLink.Application.Common;
using CuraLink.Application.DTOs;

namespace CuraLink.Application.Interfaces;

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleDto>> GetSchedulesAsync(int doctorId);
    Task<Result> AddScheduleAsync(int doctorId, ScheduleInput input);
    Task<Result> UpdateScheduleAsync(int doctorId, int scheduleId, ScheduleInput input);
    Task<Result> DeleteScheduleAsync(int doctorId, int scheduleId);
    Task<IReadOnlyList<ScheduleExceptionDto>> GetExceptionsAsync(int doctorId);
    Task<Result> AddExceptionAsync(int doctorId, ScheduleExceptionInput input);
    Task<Result> DeleteExceptionAsync(int doctorId, int exceptionId);
    Task<IReadOnlyList<TimeSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date);
}

using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly IApplicationDbContext _context;

    public ScheduleService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ScheduleDto>> GetSchedulesAsync(int doctorId)
    {
        return await _context.DoctorSchedules
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId)
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .Select(x => new ScheduleDto
            {
                Id = x.Id,
                DayOfWeek = x.DayOfWeek,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                SlotDurationMinutes = x.SlotDurationMinutes,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<Result> AddScheduleAsync(int doctorId, ScheduleInput input)
    {
        if (!await _context.Doctors.AnyAsync(x => x.Id == doctorId))
        {
            return Result.Failure("Doctor not found.");
        }

        if (input.EndTime <= input.StartTime)
        {
            return Result.Failure("End time must be after start time.");
        }

        if (input.SlotDurationMinutes < 5 || input.SlotDurationMinutes > 240)
        {
            return Result.Failure("Slot duration must be between 5 and 240 minutes.");
        }

        if (input.IsActive && await _context.DoctorSchedules.AnyAsync(x =>
                x.DoctorId == doctorId && x.IsActive && x.DayOfWeek == input.DayOfWeek &&
                x.StartTime < input.EndTime && input.StartTime < x.EndTime))
        {
            return Result.Failure("An overlapping active schedule already exists for this day.");
        }

        _context.DoctorSchedules.Add(new DoctorSchedule
        {
            DoctorId = doctorId,
            DayOfWeek = input.DayOfWeek,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            SlotDurationMinutes = input.SlotDurationMinutes,
            IsActive = input.IsActive
        });

        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> UpdateScheduleAsync(int doctorId, int scheduleId, ScheduleInput input)
    {
        var schedule = await _context.DoctorSchedules
            .SingleOrDefaultAsync(x => x.Id == scheduleId && x.DoctorId == doctorId);
        if (schedule is null)
        {
            return Result.Failure("Schedule not found.");
        }

        if (input.EndTime <= input.StartTime)
        {
            return Result.Failure("End time must be after start time.");
        }

        if (input.SlotDurationMinutes < 5 || input.SlotDurationMinutes > 240)
        {
            return Result.Failure("Slot duration must be between 5 and 240 minutes.");
        }

        if (input.IsActive && await _context.DoctorSchedules.AnyAsync(x =>
                x.Id != scheduleId && x.DoctorId == doctorId && x.IsActive && x.DayOfWeek == input.DayOfWeek &&
                x.StartTime < input.EndTime && input.StartTime < x.EndTime))
        {
            return Result.Failure("An overlapping active schedule already exists for this day.");
        }

        schedule.DayOfWeek = input.DayOfWeek;
        schedule.StartTime = input.StartTime;
        schedule.EndTime = input.EndTime;
        schedule.SlotDurationMinutes = input.SlotDurationMinutes;
        schedule.IsActive = input.IsActive;

        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteScheduleAsync(int doctorId, int scheduleId)
    {
        var schedule = await _context.DoctorSchedules
            .SingleOrDefaultAsync(x => x.Id == scheduleId && x.DoctorId == doctorId);
        if (schedule is null)
        {
            return Result.Failure("Schedule not found.");
        }

        _context.DoctorSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<IReadOnlyList<ScheduleExceptionDto>> GetExceptionsAsync(int doctorId)
    {
        return await _context.ScheduleExceptions
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId)
            .OrderBy(x => x.Date)
            .Select(x => new ScheduleExceptionDto
            {
                Id = x.Id,
                Date = x.Date,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                IsUnavailable = x.IsUnavailable,
                Reason = x.Reason
            })
            .ToListAsync();
    }

    public async Task<Result> AddExceptionAsync(int doctorId, ScheduleExceptionInput input)
    {
        if (!await _context.Doctors.AnyAsync(x => x.Id == doctorId))
        {
            return Result.Failure("Doctor not found.");
        }

        if (input.StartTime.HasValue && input.EndTime.HasValue && input.EndTime.Value <= input.StartTime.Value)
        {
            return Result.Failure("End time must be after start time.");
        }

        _context.ScheduleExceptions.Add(new ScheduleException
        {
            DoctorId = doctorId,
            Date = input.Date,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            IsUnavailable = input.IsUnavailable,
            Reason = input.Reason
        });

        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteExceptionAsync(int doctorId, int exceptionId)
    {
        var exception = await _context.ScheduleExceptions
            .SingleOrDefaultAsync(x => x.Id == exceptionId && x.DoctorId == doctorId);
        if (exception is null)
        {
            return Result.Failure("Schedule exception not found.");
        }

        _context.ScheduleExceptions.Remove(exception);
        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<IReadOnlyList<TimeSlotDto>> GetAvailableSlotsAsync(int doctorId, DateTime date)
    {
        var day = date.Date;
        var nextDay = day.AddDays(1);

        var schedules = await _context.DoctorSchedules
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId && x.IsActive && x.DayOfWeek == day.DayOfWeek)
            .ToListAsync();
        if (schedules.Count == 0)
        {
            return Array.Empty<TimeSlotDto>();
        }

        var exceptions = await _context.ScheduleExceptions
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId && x.IsUnavailable && x.Date >= day && x.Date < nextDay)
            .ToListAsync();

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId && x.StartAtUtc >= day && x.StartAtUtc < nextDay &&
                        (x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed))
            .Select(x => new { x.StartAtUtc, x.EndAtUtc })
            .ToListAsync();

        var nowUtc = DateTime.UtcNow;
        var slots = new List<TimeSlotDto>();

        foreach (var schedule in schedules)
        {
            if (schedule.SlotDurationMinutes <= 0)
            {
                continue;
            }

            var duration = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);
            for (var time = schedule.StartTime; time + duration <= schedule.EndTime; time += duration)
            {
                var start = day.Add(time);
                var end = start.Add(duration);
                if (start <= nowUtc)
                {
                    continue;
                }

                var blockedByException = exceptions.Any(x =>
                    x.StartTime is null || x.EndTime is null ||
                    (time < x.EndTime.Value && end.TimeOfDay > x.StartTime.Value));
                if (blockedByException)
                {
                    continue;
                }

                if (appointments.Any(x => AppointmentRules.Overlaps(start, end, x.StartAtUtc, x.EndAtUtc)))
                {
                    continue;
                }

                slots.Add(new TimeSlotDto
                {
                    StartUtc = start,
                    EndUtc = end,
                    IsAvailable = true
                });
            }
        }

        return slots.OrderBy(x => x.StartUtc).ToList();
    }
}

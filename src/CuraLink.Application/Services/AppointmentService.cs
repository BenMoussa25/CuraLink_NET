using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IApplicationDbContext _context;

    public AppointmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AppointmentDto>> GetDoctorAppointmentsAsync(int doctorId, AppointmentFilter filter)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(x => x.Doctor).ThenInclude(x => x.User)
            .Include(x => x.Patient).ThenInclude(x => x.User)
            .Where(x => x.DoctorId == doctorId);

        if (filter.FromUtc.HasValue)
        {
            query = query.Where(x => x.StartAtUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            query = query.Where(x => x.StartAtUtc <= filter.ToUtc.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        var totalCount = await query.CountAsync();
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

        var appointments = await query
            .OrderBy(x => x.StartAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AppointmentDto>
        {
            Items = appointments.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Result<AppointmentDto>> GetForDoctorAsync(int doctorId, int appointmentId)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .Include(x => x.Doctor).ThenInclude(x => x.User)
            .Include(x => x.Patient).ThenInclude(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == appointmentId && x.DoctorId == doctorId);

        return appointment is null
            ? Result<AppointmentDto>.Failure("Appointment not found.")
            : Result<AppointmentDto>.Ok(Map(appointment));
    }

    public async Task<Result> RequestAppointmentAsync(int patientId, AppointmentRequestDto input, DateTime nowUtc)
    {
        if (!AppointmentRules.IsInFuture(input.StartAtUtc, nowUtc))
        {
            return Result.Failure("Appointment must be in the future.");
        }

        if (!await _context.Patients.AsNoTracking().AnyAsync(x => x.Id == patientId))
        {
            return Result.Failure("Patient not found.");
        }

        var doctor = await _context.Doctors
            .AsNoTracking()
            .Where(x => x.Id == input.DoctorId)
            .Select(x => new
            {
                x.UserId,
                x.VerificationStatus,
                UserIsActive = x.User.IsActive,
                SpecialtyIsActive = x.Specialty.IsActive
            })
            .SingleOrDefaultAsync();

        if (doctor is null || !DoctorVerificationRules.CanBePubliclyListed(
                doctor.VerificationStatus, doctor.UserIsActive, doctor.SpecialtyIsActive))
        {
            return Result.Failure("Doctor is not available for appointments.");
        }

        var schedule = await GetMatchingScheduleAsync(input.DoctorId, input.StartAtUtc);

        if (schedule is null || schedule.SlotDurationMinutes <= 0)
        {
            return Result.Failure("No schedule is available for the requested time.");
        }

        var endAtUtc = input.StartAtUtc.AddMinutes(schedule.SlotDurationMinutes);
        if (!AppointmentRules.IsValidRange(input.StartAtUtc, endAtUtc))
        {
            return Result.Failure("Invalid appointment range.");
        }

        if (endAtUtc.TimeOfDay > schedule.EndTime)
        {
            return Result.Failure("Appointment exceeds the doctor's schedule.");
        }

        var blocked = await IsBlockedByScheduleExceptionAsync(input.DoctorId, input.StartAtUtc, endAtUtc);
        if (blocked)
        {
            return Result.Failure("Doctor is unavailable at the requested time.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var doctorBusy = await _context.Appointments.AnyAsync(x =>
            x.DoctorId == input.DoctorId &&
            (x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed) &&
            x.StartAtUtc < endAtUtc && x.EndAtUtc > input.StartAtUtc);
        if (doctorBusy)
        {
            return Result.Failure("The selected slot is no longer available.");
        }

        var duplicateForPatient = await _context.Appointments.AnyAsync(x =>
            x.PatientId == patientId &&
            (x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed) &&
            x.StartAtUtc < endAtUtc && x.EndAtUtc > input.StartAtUtc);
        if (duplicateForPatient)
        {
            return Result.Failure("You already have an appointment at this time.");
        }

        var appointment = new Appointment
        {
            DoctorId = input.DoctorId,
            PatientId = patientId,
            StartAtUtc = input.StartAtUtc,
            EndAtUtc = endAtUtc,
            Status = AppointmentStatus.Pending,
            PatientReason = input.PatientReason,
            CreatedAtUtc = nowUtc
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        _context.AppointmentStatusHistories.Add(new AppointmentStatusHistory
        {
            AppointmentId = appointment.Id,
            OldStatus = AppointmentStatus.Pending,
            NewStatus = AppointmentStatus.Pending,
            ChangedAtUtc = nowUtc,
            Reason = "Appointment requested"
        });

        _context.Notifications.Add(new Notification
        {
            UserId = doctor.UserId,
            Message = "A new appointment request was received.",
            Type = NotificationType.AppointmentRequest,
            RelatedEntityType = "Appointment",
            RelatedEntityId = appointment.Id.ToString(),
            IsRead = false,
            CreatedAtUtc = nowUtc
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result.Ok();
    }

    public async Task<Result> ChangeStatusAsync(
        int doctorId,
        int appointmentId,
        AppointmentStatus newStatus,
        string? reason,
        string? actorUserId,
        DateTime nowUtc)
    {
        var appointment = await _context.Appointments
            .SingleOrDefaultAsync(x => x.Id == appointmentId && x.DoctorId == doctorId);
        if (appointment is null)
        {
            return Result.Failure("Appointment not found.");
        }

        if (!AppointmentRules.IsStatusTransitionAllowed(appointment.Status, newStatus))
        {
            return Result.Failure("Status transition is not allowed.");
        }

        var oldStatus = appointment.Status;
        appointment.Status = newStatus;
        appointment.UpdatedAtUtc = nowUtc;
        if (newStatus == AppointmentStatus.Cancelled)
        {
            appointment.CancellationReason = reason;
        }

        _context.AppointmentStatusHistories.Add(new AppointmentStatusHistory
        {
            AppointmentId = appointment.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedByUserId = actorUserId,
            ChangedAtUtc = nowUtc,
            Reason = reason
        });

        var patientUserId = await _context.Patients
            .AsNoTracking()
            .Where(x => x.Id == appointment.PatientId)
            .Select(x => x.UserId)
            .SingleOrDefaultAsync();

        if (!string.IsNullOrEmpty(patientUserId))
        {
            var notificationType = newStatus switch
            {
                AppointmentStatus.Confirmed => NotificationType.AppointmentConfirmed,
                AppointmentStatus.Rejected => NotificationType.AppointmentRejected,
                AppointmentStatus.Cancelled => NotificationType.AppointmentCancelled,
                _ => NotificationType.General
            };

            _context.Notifications.Add(new Notification
            {
                UserId = patientUserId,
                Message = "Your appointment status has been updated.",
                Type = notificationType,
                RelatedEntityType = "Appointment",
                RelatedEntityId = appointment.Id.ToString(),
                IsRead = false,
                CreatedAtUtc = nowUtc
            });
        }

        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> RescheduleAsync(
        int doctorId,
        int appointmentId,
        DateTime startUtc,
        DateTime endUtc,
        string? actorUserId,
        DateTime nowUtc)
    {
        var appointment = await _context.Appointments
            .SingleOrDefaultAsync(x => x.Id == appointmentId && x.DoctorId == doctorId);
        if (appointment is null)
        {
            return Result.Failure("Appointment not found.");
        }

        if (appointment.Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed))
        {
            return Result.Failure("Appointment status cannot be rescheduled.");
        }

        if (!AppointmentRules.IsValidRange(startUtc, endUtc))
        {
            return Result.Failure("Invalid appointment range.");
        }

        if (!AppointmentRules.IsInFuture(startUtc, nowUtc))
        {
            return Result.Failure("Appointment must be in the future.");
        }

        var schedule = await GetMatchingScheduleAsync(doctorId, startUtc);
        if (schedule is null || schedule.SlotDurationMinutes <= 0)
        {
            return Result.Failure("No schedule is available for the requested time.");
        }

        if (endUtc.TimeOfDay > schedule.EndTime)
        {
            return Result.Failure("Appointment exceeds the doctor's schedule.");
        }

        if (await IsBlockedByScheduleExceptionAsync(doctorId, startUtc, endUtc))
        {
            return Result.Failure("Doctor is unavailable at the requested time.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var overlaps = await _context.Appointments.AnyAsync(x =>
            x.DoctorId == doctorId && x.Id != appointmentId &&
            (x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed) &&
            x.StartAtUtc < endUtc && x.EndAtUtc > startUtc);
        if (overlaps)
        {
            return Result.Failure("The doctor is already booked at that time.");
        }

        var duplicateForPatient = await _context.Appointments.AnyAsync(x =>
            x.PatientId == appointment.PatientId && x.Id != appointmentId &&
            (x.Status == AppointmentStatus.Pending || x.Status == AppointmentStatus.Confirmed) &&
            x.StartAtUtc < endUtc && x.EndAtUtc > startUtc);
        if (duplicateForPatient)
        {
            return Result.Failure("The patient already has an appointment at that time.");
        }

        appointment.StartAtUtc = startUtc;
        appointment.EndAtUtc = endUtc;
        appointment.UpdatedAtUtc = nowUtc;

        _context.AppointmentStatusHistories.Add(new AppointmentStatusHistory
        {
            AppointmentId = appointment.Id,
            OldStatus = appointment.Status,
            NewStatus = appointment.Status,
            ChangedByUserId = actorUserId,
            ChangedAtUtc = nowUtc,
            Reason = "Appointment rescheduled"
        });

        var patientUserId = await _context.Patients
            .AsNoTracking()
            .Where(x => x.Id == appointment.PatientId)
            .Select(x => x.UserId)
            .SingleOrDefaultAsync();

        if (!string.IsNullOrEmpty(patientUserId))
        {
            _context.Notifications.Add(new Notification
            {
                UserId = patientUserId,
                Message = "Your appointment has been rescheduled.",
                Type = NotificationType.General,
                RelatedEntityType = "Appointment",
                RelatedEntityId = appointment.Id.ToString(),
                IsRead = false,
                CreatedAtUtc = nowUtc
            });
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return Result.Ok();
    }

    private async Task<DoctorSchedule?> GetMatchingScheduleAsync(int doctorId, DateTime startUtc)
    {
        return await _context.DoctorSchedules
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId && x.IsActive &&
                        x.DayOfWeek == startUtc.DayOfWeek &&
                        x.StartTime <= startUtc.TimeOfDay &&
                        x.EndTime > startUtc.TimeOfDay)
            .FirstOrDefaultAsync();
    }

    private async Task<bool> IsBlockedByScheduleExceptionAsync(int doctorId, DateTime startUtc, DateTime endUtc)
    {
        var day = startUtc.Date;
        var timeOfDay = startUtc.TimeOfDay;
        return await _context.ScheduleExceptions
            .AsNoTracking()
            .AnyAsync(x => x.DoctorId == doctorId && x.IsUnavailable &&
                           x.Date >= day && x.Date < day.AddDays(1) &&
                           (x.StartTime == null || x.EndTime == null ||
                            (timeOfDay < x.EndTime.Value && endUtc.TimeOfDay > x.StartTime.Value)));
    }

    private static AppointmentDto Map(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}".Trim(),
            PatientId = appointment.PatientId,
            PatientName = $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}".Trim(),
            StartAtUtc = appointment.StartAtUtc,
            EndAtUtc = appointment.EndAtUtc,
            Status = appointment.Status,
            PatientReason = appointment.PatientReason,
            CancellationReason = appointment.CancellationReason,
            CreatedAtUtc = appointment.CreatedAtUtc,
            UpdatedAtUtc = appointment.UpdatedAtUtc
        };
    }
}

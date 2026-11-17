using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Services;

public sealed class DoctorDashboardService : IDoctorDashboardService
{
    private readonly IApplicationDbContext _context;

    public DoctorDashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorDashboardDto> GetDashboardAsync(int doctorId, DateTime nowUtc)
    {
        var dayStart = nowUtc.Date;
        var dayEnd = dayStart.AddDays(1);

        var today = await _context.Appointments
            .AsNoTracking()
            .Include(x => x.Doctor).ThenInclude(x => x.User)
            .Include(x => x.Patient).ThenInclude(x => x.User)
            .Where(x => x.DoctorId == doctorId && x.StartAtUtc >= dayStart && x.StartAtUtc < dayEnd)
            .OrderBy(x => x.StartAtUtc)
            .ToListAsync();

        var pending = await _context.Appointments
            .AsNoTracking()
            .Include(x => x.Doctor).ThenInclude(x => x.User)
            .Include(x => x.Patient).ThenInclude(x => x.User)
            .Where(x => x.DoctorId == doctorId && x.Status == AppointmentStatus.Pending)
            .OrderBy(x => x.StartAtUtc)
            .ToListAsync();

        var upcomingCount = await _context.Appointments
            .AsNoTracking()
            .CountAsync(x => x.DoctorId == doctorId &&
                             x.Status == AppointmentStatus.Confirmed &&
                             x.StartAtUtc > nowUtc);

        var patientCount = await _context.Appointments
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId)
            .Select(x => x.PatientId)
            .Distinct()
            .CountAsync();

        return new DoctorDashboardDto
        {
            TodayAppointmentCount = today.Count,
            PendingRequestCount = pending.Count,
            UpcomingAppointmentCount = upcomingCount,
            PatientCount = patientCount,
            TodayAppointments = today.Select(Map).ToList(),
            PendingRequests = pending.Select(Map).ToList()
        };
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

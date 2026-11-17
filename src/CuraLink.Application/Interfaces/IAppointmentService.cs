using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Domain.Enums;

namespace CuraLink.Application.Interfaces;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentDto>> GetDoctorAppointmentsAsync(int doctorId, AppointmentFilter filter);
    Task<Result<AppointmentDto>> GetForDoctorAsync(int doctorId, int appointmentId);
    Task<Result> RequestAppointmentAsync(int patientId, AppointmentRequestDto input, DateTime nowUtc);
    Task<Result> ChangeStatusAsync(int doctorId, int appointmentId, AppointmentStatus newStatus, string? reason, string? actorUserId, DateTime nowUtc);
    Task<Result> RescheduleAsync(int doctorId, int appointmentId, DateTime startUtc, DateTime endUtc, string? actorUserId, DateTime nowUtc);
}

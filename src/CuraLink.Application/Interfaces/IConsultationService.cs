using CuraLink.Application.Common;
using CuraLink.Application.DTOs;

namespace CuraLink.Application.Interfaces;

public interface IConsultationService
{
    Task<IReadOnlyList<ConsultationDto>> GetForDoctorAsync(int doctorId);
    Task<Result<ConsultationDto>> GetForDoctorAsync(int doctorId, int consultationId);
    Task<Result<int>> CreateAsync(int doctorId, ConsultationInput input, string? actorUserId, DateTime nowUtc);
    Task<Result> UpdateAsync(int doctorId, int consultationId, ConsultationInput input, string? actorUserId, DateTime nowUtc);
    Task<Result> FinalizeAsync(int doctorId, int consultationId, string? actorUserId, DateTime nowUtc);
    Task<Result> ShareWithPatientAsync(int doctorId, int consultationId, string? actorUserId, DateTime nowUtc);
}

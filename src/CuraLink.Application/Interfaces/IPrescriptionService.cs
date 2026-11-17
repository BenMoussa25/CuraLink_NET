using CuraLink.Application.Common;
using CuraLink.Application.DTOs;

namespace CuraLink.Application.Interfaces;

public interface IPrescriptionService
{
    Task<IReadOnlyList<PrescriptionDto>> GetForDoctorAsync(int doctorId);
    Task<Result<int>> IssueAsync(int doctorId, PrescriptionInput input, string? actorUserId, DateTime nowUtc);
    Task<Result> CancelAsync(int doctorId, int prescriptionId, string? actorUserId, DateTime nowUtc);
}

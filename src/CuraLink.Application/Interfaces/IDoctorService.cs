using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Domain.Entities;

namespace CuraLink.Application.Interfaces;

public interface IDoctorService
{
    Task<Doctor?> GetByUserIdAsync(string userId);
    Task<Doctor?> GetCurrentDoctorAsync();
    Task<Result<DoctorProfileDto>> GetProfileAsync(int doctorId);
    Task<Result> UpdateProfileAsync(int doctorId, DoctorProfileInput input);
    Task<IReadOnlyList<SpecialtyDto>> GetActiveSpecialtiesAsync();
}

using CuraLink.Application.DTOs;

namespace CuraLink.Application.Interfaces;

public interface IDoctorDashboardService
{
    Task<DoctorDashboardDto> GetDashboardAsync(int doctorId, DateTime nowUtc);
}

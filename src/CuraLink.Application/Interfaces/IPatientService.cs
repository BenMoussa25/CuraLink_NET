using CuraLink.Application.Common;
using CuraLink.Application.DTOs;

namespace CuraLink.Application.Interfaces;

public interface IPatientService
{
    Task<IReadOnlyList<PatientSummaryDto>> GetDoctorPatientsAsync(int doctorId);
    Task<Result<PatientDetailDto>> GetPatientForDoctorAsync(int doctorId, int patientId);
}

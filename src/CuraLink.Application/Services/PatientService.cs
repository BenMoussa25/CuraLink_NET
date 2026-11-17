using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Services;

public sealed class PatientService : IPatientService
{
    private readonly IApplicationDbContext _context;

    public PatientService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PatientSummaryDto>> GetDoctorPatientsAsync(int doctorId)
    {
        return await _context.Patients
            .AsNoTracking()
            .Where(x => x.Appointments.Any(a => a.DoctorId == doctorId))
            .OrderBy(x => x.User.LastName)
            .ThenBy(x => x.User.FirstName)
            .Select(x => new PatientSummaryDto
            {
                Id = x.Id,
                FullName = (x.User.FirstName + " " + x.User.LastName).Trim(),
                Email = x.User.Email ?? string.Empty,
                City = null,
                AppointmentCount = x.Appointments.Count(a => a.DoctorId == doctorId),
                LastAppointmentUtc = x.Appointments
                    .Where(a => a.DoctorId == doctorId)
                    .Select(a => (DateTime?)a.StartAtUtc)
                    .Max()
            })
            .ToListAsync();
    }

    public async Task<Result<PatientDetailDto>> GetPatientForDoctorAsync(int doctorId, int patientId)
    {
        var patient = await _context.Patients
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.MedicalRecord).ThenInclude(x => x!.Allergies)
            .Include(x => x.MedicalRecord).ThenInclude(x => x!.Conditions)
            .Include(x => x.Consultations).ThenInclude(x => x.Doctor).ThenInclude(x => x.User)
            .Include(x => x.Prescriptions).ThenInclude(x => x.Items)
            .Include(x => x.Documents)
            .SingleOrDefaultAsync(x => x.Id == patientId && x.Appointments.Any(a => a.DoctorId == doctorId));

        if (patient is null)
        {
            return Result<PatientDetailDto>.Failure("Patient not found.");
        }

        var hasAppointmentRelationship = await _context.Appointments
            .AsNoTracking()
            .AnyAsync(a => a.DoctorId == doctorId && a.PatientId == patientId);

        var fullName = $"{patient.User.FirstName} {patient.User.LastName}".Trim();
        var dto = new PatientDetailDto
        {
            Id = patient.Id,
            FullName = fullName,
            Email = patient.User.Email ?? string.Empty,
            PhoneNumber = patient.User.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            Address = patient.Address,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            Allergies = patient.MedicalRecord?.Allergies
                .Select(x => new AllergyDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Reaction = x.Reaction,
                    Notes = x.Notes
                })
                .ToList() ?? new List<AllergyDto>(),
            Conditions = patient.MedicalRecord?.Conditions
                .Select(x => new MedicalConditionDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Notes = x.Notes,
                    RecordedAtUtc = x.RecordedAtUtc
                })
                .ToList() ?? new List<MedicalConditionDto>(),
            Consultations = patient.Consultations
                .Where(x => x.DoctorId == doctorId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => MapConsultation(x, fullName))
                .ToList(),
            Prescriptions = patient.Prescriptions
                .Where(x => x.DoctorId == doctorId)
                .OrderByDescending(x => x.IssuedAtUtc)
                .Select(x => MapPrescription(x, fullName))
                .ToList(),
            Documents = patient.Documents
                .Where(x => x.DoctorId == doctorId ||
                            (hasAppointmentRelationship && x.VisibilityStatus == VisibilityStatus.SharedWithDoctor))
                .OrderByDescending(x => x.UploadedAtUtc)
                .Select(x => MapDocument(x, fullName))
                .ToList()
        };

        return Result<PatientDetailDto>.Ok(dto);
    }

    private static ConsultationDto MapConsultation(Consultation consultation, string patientName)
    {
        return new ConsultationDto
        {
            Id = consultation.Id,
            AppointmentId = consultation.AppointmentId,
            PatientId = consultation.PatientId,
            PatientName = patientName,
            DoctorId = consultation.DoctorId,
            DoctorName = $"{consultation.Doctor.User.FirstName} {consultation.Doctor.User.LastName}".Trim(),
            ConsultationDate = consultation.CreatedAtUtc,
            Reason = consultation.Reason,
            ReportedSymptoms = consultation.ReportedSymptoms,
            ClinicalObservations = consultation.ClinicalObservations,
            Diagnosis = consultation.Diagnosis,
            TreatmentPlan = consultation.TreatmentPlan,
            Recommendations = consultation.Recommendations,
            FollowUpDate = consultation.FollowUpDate,
            Status = consultation.Status,
            IsSharedWithPatient = consultation.IsSharedWithPatient,
            CreatedAtUtc = consultation.CreatedAtUtc,
            UpdatedAtUtc = consultation.UpdatedAtUtc,
            FinalizedAtUtc = consultation.FinalizedAtUtc
        };
    }

    private static PrescriptionDto MapPrescription(Prescription prescription, string patientName)
    {
        return new PrescriptionDto
        {
            Id = prescription.Id,
            ConsultationId = prescription.ConsultationId,
            PatientId = prescription.PatientId,
            PatientName = patientName,
            DoctorId = prescription.DoctorId,
            IssuedAtUtc = prescription.IssuedAtUtc,
            GeneralInstructions = prescription.GeneralInstructions,
            Status = prescription.Status,
            IsSharedWithPatient = prescription.IsSharedWithPatient,
            Items = prescription.Items
                .Select(x => new PrescriptionItemDto
                {
                    Id = x.Id,
                    MedicineName = x.MedicineName,
                    Strength = x.Strength,
                    DosageInstructions = x.DosageInstructions,
                    Frequency = x.Frequency,
                    Duration = x.Duration,
                    Route = x.Route,
                    AdditionalInstructions = x.AdditionalInstructions
                })
                .ToList()
        };
    }

    private static MedicalDocumentDto MapDocument(MedicalDocument document, string patientName)
    {
        return new MedicalDocumentDto
        {
            Id = document.Id,
            PatientId = document.PatientId,
            PatientName = patientName,
            DocumentType = document.DocumentType,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            VisibilityStatus = document.VisibilityStatus,
            UploadedAtUtc = document.UploadedAtUtc
        };
    }
}

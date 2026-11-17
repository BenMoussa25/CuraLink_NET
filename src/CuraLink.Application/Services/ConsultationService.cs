using System.Text.Json;
using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Services;

public sealed class ConsultationService : IConsultationService
{
    private readonly IApplicationDbContext _context;

    public ConsultationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ConsultationDto>> GetForDoctorAsync(int doctorId)
    {
        var consultations = await _context.Consultations
            .AsNoTracking()
            .Include(x => x.Appointment)
            .Include(x => x.Patient).ThenInclude(x => x.User)
            .Include(x => x.Doctor).ThenInclude(x => x.User)
            .Where(x => x.DoctorId == doctorId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return consultations.Select(Map).ToList();
    }

    public async Task<Result<ConsultationDto>> GetForDoctorAsync(int doctorId, int consultationId)
    {
        var consultation = await _context.Consultations
            .AsNoTracking()
            .Include(x => x.Appointment)
            .Include(x => x.Patient).ThenInclude(x => x.User)
            .Include(x => x.Doctor).ThenInclude(x => x.User)
            .SingleOrDefaultAsync(x => x.Id == consultationId && x.DoctorId == doctorId);

        return consultation is null
            ? Result<ConsultationDto>.Failure("Consultation not found.")
            : Result<ConsultationDto>.Ok(Map(consultation));
    }

    public async Task<Result<int>> CreateAsync(
        int doctorId,
        ConsultationInput input,
        string? actorUserId,
        DateTime nowUtc)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == input.AppointmentId && x.DoctorId == doctorId);
        if (appointment is null)
        {
            return Result<int>.Failure("Appointment not found for this doctor.");
        }

        if (await _context.Consultations.AnyAsync(x => x.AppointmentId == input.AppointmentId))
        {
            return Result<int>.Failure("A consultation already exists for this appointment.");
        }

        var consultation = new Consultation
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = doctorId,
            Reason = input.Reason,
            ReportedSymptoms = input.ReportedSymptoms,
            ClinicalObservations = input.ClinicalObservations,
            Diagnosis = input.Diagnosis,
            TreatmentPlan = input.TreatmentPlan,
            Recommendations = input.Recommendations,
            FollowUpDate = input.FollowUpDate,
            Status = ConsultationStatus.Draft,
            IsSharedWithPatient = false,
            CreatedAtUtc = nowUtc
        };

        _context.Consultations.Add(consultation);

        _context.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = "ConsultationCreated",
            EntityType = "Consultation",
            EntityId = consultation.Id.ToString(),
            OccurredAtUtc = nowUtc,
            MetadataJson = JsonSerializer.Serialize(new { appointmentId = appointment.Id })
        });

        await _context.SaveChangesAsync();
        return Result<int>.Ok(consultation.Id);
    }

    public async Task<Result> UpdateAsync(
        int doctorId,
        int consultationId,
        ConsultationInput input,
        string? actorUserId,
        DateTime nowUtc)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == input.AppointmentId && x.DoctorId == doctorId);
        if (appointment is null)
        {
            return Result.Failure("Appointment not found for this doctor.");
        }

        var consultation = await _context.Consultations
            .SingleOrDefaultAsync(x => x.Id == consultationId && x.DoctorId == doctorId);
        if (consultation is null)
        {
            return Result.Failure("Consultation not found.");
        }

        if (consultation.Status == ConsultationStatus.Finalized)
        {
            return Result.Failure("Finalized consultations cannot be edited.");
        }

        consultation.Reason = input.Reason;
        consultation.ReportedSymptoms = input.ReportedSymptoms;
        consultation.ClinicalObservations = input.ClinicalObservations;
        consultation.Diagnosis = input.Diagnosis;
        consultation.TreatmentPlan = input.TreatmentPlan;
        consultation.Recommendations = input.Recommendations;
        consultation.FollowUpDate = input.FollowUpDate;
        consultation.UpdatedAtUtc = nowUtc;

        _context.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = "ConsultationUpdated",
            EntityType = "Consultation",
            EntityId = consultation.Id.ToString(),
            OccurredAtUtc = nowUtc,
            MetadataJson = JsonSerializer.Serialize(new { appointmentId = appointment.Id })
        });

        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> FinalizeAsync(
        int doctorId,
        int consultationId,
        string? actorUserId,
        DateTime nowUtc)
    {
        var consultation = await _context.Consultations
            .SingleOrDefaultAsync(x => x.Id == consultationId && x.DoctorId == doctorId);
        if (consultation is null)
        {
            return Result.Failure("Consultation not found.");
        }

        if (!await _context.Appointments
                .AsNoTracking()
                .AnyAsync(x => x.Id == consultation.AppointmentId && x.DoctorId == doctorId))
        {
            return Result.Failure("Appointment not found for this doctor.");
        }

        if (consultation.Status == ConsultationStatus.Finalized)
        {
            return Result.Failure("Consultation is already finalized.");
        }

        consultation.Status = ConsultationStatus.Finalized;
        consultation.FinalizedAtUtc = nowUtc;
        consultation.UpdatedAtUtc = nowUtc;

        _context.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = "ConsultationFinalized",
            EntityType = "Consultation",
            EntityId = consultation.Id.ToString(),
            OccurredAtUtc = nowUtc,
            MetadataJson = JsonSerializer.Serialize(new { status = ConsultationStatus.Finalized.ToString() })
        });

        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> ShareWithPatientAsync(
        int doctorId,
        int consultationId,
        string? actorUserId,
        DateTime nowUtc)
    {
        var consultation = await _context.Consultations
            .SingleOrDefaultAsync(x => x.Id == consultationId && x.DoctorId == doctorId);
        if (consultation is null)
        {
            return Result.Failure("Consultation not found.");
        }

        if (consultation.Status != ConsultationStatus.Finalized)
        {
            return Result.Failure("Only finalized consultations can be shared.");
        }

        if (!await _context.Appointments
                .AsNoTracking()
                .AnyAsync(x => x.Id == consultation.AppointmentId && x.DoctorId == doctorId))
        {
            return Result.Failure("Appointment not found for this doctor.");
        }

        consultation.IsSharedWithPatient = true;
        consultation.UpdatedAtUtc = nowUtc;

        _context.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = "ConsultationSharedWithPatient",
            EntityType = "Consultation",
            EntityId = consultation.Id.ToString(),
            OccurredAtUtc = nowUtc,
            MetadataJson = JsonSerializer.Serialize(new { sharedWithPatient = true })
        });

        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    private static ConsultationDto Map(Consultation consultation)
    {
        return new ConsultationDto
        {
            Id = consultation.Id,
            AppointmentId = consultation.AppointmentId,
            PatientId = consultation.PatientId,
            PatientName = $"{consultation.Patient.User.FirstName} {consultation.Patient.User.LastName}".Trim(),
            DoctorId = consultation.DoctorId,
            DoctorName = $"{consultation.Doctor.User.FirstName} {consultation.Doctor.User.LastName}".Trim(),
            ConsultationDate = consultation.Appointment.StartAtUtc,
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
}

using System.Text.Json;
using CuraLink.Application.Common;
using CuraLink.Application.DTOs;
using CuraLink.Application.Interfaces;
using CuraLink.Domain.Common;
using CuraLink.Domain.Entities;
using CuraLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Application.Services;

public sealed class PrescriptionService : IPrescriptionService
{
    private readonly IApplicationDbContext _context;

    public PrescriptionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PrescriptionDto>> GetForDoctorAsync(int doctorId)
    {
        var prescriptions = await _context.Prescriptions
            .AsNoTracking()
            .Include(x => x.Patient).ThenInclude(x => x.User)
            .Include(x => x.Items)
            .Where(x => x.DoctorId == doctorId)
            .OrderByDescending(x => x.IssuedAtUtc)
            .ToListAsync();

        return prescriptions.Select(Map).ToList();
    }

    public async Task<Result<int>> IssueAsync(
        int doctorId,
        PrescriptionInput input,
        string? actorUserId,
        DateTime nowUtc)
    {
        var consultation = await _context.Consultations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == input.ConsultationId && x.DoctorId == doctorId);
        if (consultation is null || consultation.DoctorId != doctorId)
        {
            return Result<int>.Failure("Consultation not found for this doctor.");
        }

        if (consultation.Status != ConsultationStatus.Finalized)
        {
            return Result<int>.Failure("Only finalized consultations can be prescribed.");
        }

        if (await _context.Prescriptions.AnyAsync(x => x.ConsultationId == consultation.Id &&
                                                       (x.Status == PrescriptionStatus.Draft ||
                                                        x.Status == PrescriptionStatus.Issued)))
        {
            return Result<int>.Failure("An active prescription already exists for this consultation.");
        }

        var items = input.Items.Select(x => new PrescriptionItem
        {
            MedicineName = x.MedicineName,
            Strength = x.Strength,
            DosageInstructions = x.DosageInstructions,
            Frequency = x.Frequency,
            Duration = x.Duration,
            Route = x.Route,
            AdditionalInstructions = x.AdditionalInstructions
        }).ToList();

        if (!PrescriptionRules.HasValidItems(items))
        {
            return Result<int>.Failure("Prescription must contain at least one item with a name and instructions.");
        }

        var prescription = new Prescription
        {
            ConsultationId = consultation.Id,
            PatientId = consultation.PatientId,
            DoctorId = doctorId,
            IssuedAtUtc = nowUtc,
            GeneralInstructions = input.GeneralInstructions,
            Status = PrescriptionStatus.Issued,
            IsSharedWithPatient = true,
            Items = items
        };

        _context.Prescriptions.Add(prescription);

        _context.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = "PrescriptionIssued",
            EntityType = "Prescription",
            EntityId = prescription.Id.ToString(),
            OccurredAtUtc = nowUtc,
            MetadataJson = JsonSerializer.Serialize(new
            {
                consultationId = consultation.Id,
                itemCount = items.Count
            })
        });

        await _context.SaveChangesAsync();
        return Result<int>.Ok(prescription.Id);
    }

    public async Task<Result> CancelAsync(
        int doctorId,
        int prescriptionId,
        string? actorUserId,
        DateTime nowUtc)
    {
        var prescription = await _context.Prescriptions
            .SingleOrDefaultAsync(x => x.Id == prescriptionId && x.DoctorId == doctorId);
        if (prescription is null)
        {
            return Result.Failure("Prescription not found.");
        }

        if (prescription.Status == PrescriptionStatus.Cancelled)
        {
            return Result.Ok();
        }

        prescription.Status = PrescriptionStatus.Cancelled;

        _context.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = "PrescriptionCancelled",
            EntityType = "Prescription",
            EntityId = prescription.Id.ToString(),
            OccurredAtUtc = nowUtc,
            MetadataJson = JsonSerializer.Serialize(new { status = PrescriptionStatus.Cancelled.ToString() })
        });

        await _context.SaveChangesAsync();
        return Result.Ok();
    }

    private static PrescriptionDto Map(Prescription prescription)
    {
        return new PrescriptionDto
        {
            Id = prescription.Id,
            ConsultationId = prescription.ConsultationId,
            PatientId = prescription.PatientId,
            PatientName = $"{prescription.Patient.User.FirstName} {prescription.Patient.User.LastName}".Trim(),
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
}

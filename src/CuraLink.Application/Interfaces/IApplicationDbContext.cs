using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CuraLink.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Specialty> Specialties { get; }
    DbSet<DoctorSchedule> DoctorSchedules { get; }
    DbSet<ScheduleException> ScheduleExceptions { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<AppointmentStatusHistory> AppointmentStatusHistories { get; }
    DbSet<MedicalRecord> MedicalRecords { get; }
    DbSet<Allergy> Allergies { get; }
    DbSet<MedicalCondition> MedicalConditions { get; }
    DbSet<Consultation> Consultations { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<PrescriptionItem> PrescriptionItems { get; }
    DbSet<MedicalDocument> MedicalDocuments { get; }
    DbSet<PatientNote> PatientNotes { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

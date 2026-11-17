using CuraLink.Application.Interfaces;
using CuraLink.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CuraLink.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Specialty> Specialties => Set<Specialty>();

    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();

    public DbSet<ScheduleException> ScheduleExceptions => Set<ScheduleException>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<AppointmentStatusHistory> AppointmentStatusHistories => Set<AppointmentStatusHistory>();

    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();

    public DbSet<Allergy> Allergies => Set<Allergy>();

    public DbSet<MedicalCondition> MedicalConditions => Set<MedicalCondition>();

    public DbSet<Consultation> Consultations => Set<Consultation>();

    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    public DbSet<MedicalDocument> MedicalDocuments => Set<MedicalDocument>();

    public DbSet<PatientNote> PatientNotes => Set<PatientNote>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
            {
                continue;
            }

            var createdProperty = entry.Metadata.FindProperty("CreatedAtUtc");
            if (entry.State == EntityState.Added && createdProperty is not null)
            {
                var createdEntry = entry.Property(createdProperty.Name);
                if (createdEntry.CurrentValue is DateTime created && created == default)
                {
                    createdEntry.CurrentValue = now;
                }
            }

            var updatedProperty = entry.Metadata.FindProperty("UpdatedAtUtc");
            if (entry.State == EntityState.Modified && updatedProperty is not null)
            {
                var updatedEntry = entry.Property(updatedProperty.Name);
                if (!updatedEntry.IsModified)
                {
                    updatedEntry.CurrentValue = now;
                }
            }
        }
    }
}

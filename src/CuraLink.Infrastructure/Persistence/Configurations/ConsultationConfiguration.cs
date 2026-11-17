using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("Consultations");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AppointmentId).IsUnique();
        builder.HasIndex(x => new { x.PatientId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.DoctorId, x.CreatedAtUtc });

        builder.Property(x => x.Reason).IsRequired().HasMaxLength(300);
        builder.Property(x => x.ReportedSymptoms).HasMaxLength(4000);
        builder.Property(x => x.ClinicalObservations).HasMaxLength(4000);
        builder.Property(x => x.Diagnosis).HasMaxLength(4000);
        builder.Property(x => x.TreatmentPlan).HasMaxLength(4000);
        builder.Property(x => x.Recommendations).HasMaxLength(4000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne(x => x.Appointment).WithOne(x => x.Consultation).HasForeignKey<Consultation>(x => x.AppointmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Patient).WithMany(x => x.Consultations).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Doctor).WithMany(x => x.Consultations).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
    }
}

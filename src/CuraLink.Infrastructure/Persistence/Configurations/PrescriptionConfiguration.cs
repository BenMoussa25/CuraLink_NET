using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.ToTable("Prescriptions");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ConsultationId);
        builder.HasIndex(x => new { x.PatientId, x.IssuedAtUtc });
        builder.HasIndex(x => new { x.DoctorId, x.IssuedAtUtc });

        builder.Property(x => x.IssuedAtUtc).IsRequired();
        builder.Property(x => x.GeneralInstructions).HasMaxLength(1000);

        builder.HasOne(x => x.Consultation).WithMany(x => x.Prescriptions).HasForeignKey(x => x.ConsultationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Patient).WithMany(x => x.Prescriptions).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Doctor).WithMany(x => x.Prescriptions).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
    }
}

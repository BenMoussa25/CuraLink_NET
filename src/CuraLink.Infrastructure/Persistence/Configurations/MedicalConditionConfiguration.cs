using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class MedicalConditionConfiguration : IEntityTypeConfiguration<MedicalCondition>
{
    public void Configure(EntityTypeBuilder<MedicalCondition> builder)
    {
        builder.ToTable("MedicalConditions");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.MedicalRecordId);
        builder.HasIndex(x => x.RecordedByDoctorId);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.RecordedAtUtc).IsRequired();

        builder.HasOne(x => x.MedicalRecord).WithMany(x => x.Conditions).HasForeignKey(x => x.MedicalRecordId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RecordedByDoctor).WithMany().HasForeignKey(x => x.RecordedByDoctorId).OnDelete(DeleteBehavior.Restrict);
    }
}

using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.ToTable("MedicalRecords");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.PatientId).IsUnique();

        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne(x => x.Patient).WithOne(x => x.MedicalRecord).HasForeignKey<MedicalRecord>(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class MedicalDocumentConfiguration : IEntityTypeConfiguration<MedicalDocument>
{
    public void Configure(EntityTypeBuilder<MedicalDocument> builder)
    {
        builder.ToTable("MedicalDocuments");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.DoctorId);
        builder.HasIndex(x => x.StorageKey).IsUnique();

        builder.Property(x => x.UploadedByUserId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(260);
        builder.Property(x => x.StoredFileName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StorageKey).IsRequired().HasMaxLength(500);
        builder.Property(x => x.UploadedAtUtc).IsRequired();

        builder.HasOne(x => x.Patient).WithMany(x => x.Documents).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Doctor).WithMany(x => x.Documents).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
    }
}

using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.PrescriptionId);

        builder.Property(x => x.MedicineName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Strength).HasMaxLength(100);
        builder.Property(x => x.DosageInstructions).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Frequency).HasMaxLength(100);
        builder.Property(x => x.Duration).HasMaxLength(100);
        builder.Property(x => x.Route).HasMaxLength(100);
        builder.Property(x => x.AdditionalInstructions).HasMaxLength(500);

        builder.HasOne(x => x.Prescription).WithMany(x => x.Items).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
    }
}

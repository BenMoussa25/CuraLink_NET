using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class AllergyConfiguration : IEntityTypeConfiguration<Allergy>
{
    public void Configure(EntityTypeBuilder<Allergy> builder)
    {
        builder.ToTable("Allergies");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.MedicalRecordId);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Reaction).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasOne(x => x.MedicalRecord).WithMany(x => x.Allergies).HasForeignKey(x => x.MedicalRecordId).OnDelete(DeleteBehavior.Restrict);
    }
}

using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.SpecialtyId);
        builder.HasIndex(x => x.VerificationStatus);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.Biography).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.ClinicName).HasMaxLength(150);
        builder.Property(x => x.ClinicAddress).HasMaxLength(300);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.PublicPhone).HasMaxLength(30);
        builder.Property(x => x.ConsultationFee).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CurrencyCode).HasMaxLength(8);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ProfileImagePath).HasMaxLength(300);

        builder.HasOne(x => x.User).WithOne(x => x.Doctor).HasForeignKey<Doctor>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Specialty).WithMany(x => x.Doctors).HasForeignKey(x => x.SpecialtyId).OnDelete(DeleteBehavior.Restrict);
    }
}

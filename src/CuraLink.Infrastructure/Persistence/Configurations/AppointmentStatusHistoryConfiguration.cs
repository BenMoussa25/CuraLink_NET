using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class AppointmentStatusHistoryConfiguration : IEntityTypeConfiguration<AppointmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<AppointmentStatusHistory> builder)
    {
        builder.ToTable("AppointmentStatusHistories");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AppointmentId);
        builder.HasIndex(x => x.ChangedAtUtc);

        builder.Property(x => x.ChangedByUserId).HasMaxLength(450);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ChangedAtUtc).IsRequired();

        builder.HasOne(x => x.Appointment).WithMany(x => x.StatusHistory).HasForeignKey(x => x.AppointmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.SetNull);
    }
}

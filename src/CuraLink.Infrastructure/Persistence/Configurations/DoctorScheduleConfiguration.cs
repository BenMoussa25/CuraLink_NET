using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
    {
        builder.ToTable("DoctorSchedules");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.DoctorId);

        builder.Property(x => x.StartTime).IsRequired().HasColumnType("time");
        builder.Property(x => x.EndTime).IsRequired().HasColumnType("time");
        builder.Property(x => x.SlotDurationMinutes).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasOne(x => x.Doctor).WithMany(x => x.Schedules).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Cascade);
    }
}

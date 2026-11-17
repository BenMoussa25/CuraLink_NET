using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class ScheduleExceptionConfiguration : IEntityTypeConfiguration<ScheduleException>
{
    public void Configure(EntityTypeBuilder<ScheduleException> builder)
    {
        builder.ToTable("ScheduleExceptions");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.DoctorId, x.Date });

        builder.Property(x => x.Date).IsRequired().HasColumnType("date");
        builder.Property(x => x.StartTime).HasColumnType("time");
        builder.Property(x => x.EndTime).HasColumnType("time");
        builder.Property(x => x.Reason).HasMaxLength(300);

        builder.HasOne(x => x.Doctor).WithMany(x => x.Exceptions).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Cascade);
    }
}

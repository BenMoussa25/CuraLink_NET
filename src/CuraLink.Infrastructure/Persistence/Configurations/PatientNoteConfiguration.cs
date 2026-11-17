using CuraLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CuraLink.Infrastructure.Persistence.Configurations;

public sealed class PatientNoteConfiguration : IEntityTypeConfiguration<PatientNote>
{
    public void Configure(EntityTypeBuilder<PatientNote> builder)
    {
        builder.ToTable("PatientNotes");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.PatientId);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasOne(x => x.Patient).WithMany(x => x.Notes).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
    }
}

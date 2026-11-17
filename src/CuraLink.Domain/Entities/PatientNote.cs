namespace CuraLink.Domain.Entities;

public class PatientNote
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? ReminderAtUtc { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsSharedWithDoctor { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

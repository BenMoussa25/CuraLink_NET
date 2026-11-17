namespace CuraLink.Domain.Entities;

public class ScheduleException
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public DateTime Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsUnavailable { get; set; }
    public string? Reason { get; set; }
}

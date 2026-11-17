namespace CuraLink.Application.DTOs;

public class ScheduleDto
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public bool IsActive { get; set; }
}

public class ScheduleInput
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    [System.ComponentModel.DataAnnotations.Range(5, 240)]
    public int SlotDurationMinutes { get; set; }

    public bool IsActive { get; set; }
}

public class ScheduleExceptionDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsUnavailable { get; set; }
    public string? Reason { get; set; }
}

public class ScheduleExceptionInput
{
    public DateTime Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool IsUnavailable { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(300)]
    public string? Reason { get; set; }
}

public class TimeSlotDto
{
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsAvailable { get; set; }
}

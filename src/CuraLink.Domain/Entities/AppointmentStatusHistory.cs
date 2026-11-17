using CuraLink.Domain.Enums;

namespace CuraLink.Domain.Entities;

public class AppointmentStatusHistory
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public AppointmentStatus OldStatus { get; set; }
    public AppointmentStatus NewStatus { get; set; }
    public string? ChangedByUserId { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string? Reason { get; set; }
}

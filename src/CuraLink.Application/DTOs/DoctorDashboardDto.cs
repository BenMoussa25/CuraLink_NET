namespace CuraLink.Application.DTOs;

public class DoctorDashboardDto
{
    public int TodayAppointmentCount { get; set; }
    public int PendingRequestCount { get; set; }
    public int UpcomingAppointmentCount { get; set; }
    public int PatientCount { get; set; }
    public IReadOnlyList<AppointmentDto> TodayAppointments { get; set; } = new List<AppointmentDto>();
    public IReadOnlyList<AppointmentDto> PendingRequests { get; set; } = new List<AppointmentDto>();
}

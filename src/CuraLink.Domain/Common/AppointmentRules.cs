using CuraLink.Domain.Enums;

namespace CuraLink.Domain.Common;

public static class AppointmentRules
{
    public static bool IsValidRange(DateTime startUtc, DateTime endUtc) => endUtc > startUtc;

    public static bool IsInFuture(DateTime startUtc, DateTime nowUtc) => startUtc > nowUtc;

    public static bool Overlaps(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd) =>
        aStart < bEnd && bStart < aEnd;

    public static bool IsStatusTransitionAllowed(AppointmentStatus current, AppointmentStatus next) =>
        current switch
        {
            AppointmentStatus.Pending => next is AppointmentStatus.Confirmed
                or AppointmentStatus.Rejected
                or AppointmentStatus.Cancelled,
            AppointmentStatus.Confirmed => next is AppointmentStatus.Completed
                or AppointmentStatus.Cancelled
                or AppointmentStatus.NoShow,
            _ => false
        };
}

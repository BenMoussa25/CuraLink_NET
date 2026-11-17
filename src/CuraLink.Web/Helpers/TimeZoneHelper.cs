namespace CuraLink.Web.Helpers;

public static class TimeZoneHelper
{
    public static TimeZoneInfo Resolve(IConfiguration configuration)
    {
        var candidates = new[]
        {
            configuration["App:TimeZone"],
            "Africa/Tunis",
            "Tunis Standard Time"
        };

        foreach (var id in candidates)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }

    public static string Format(DateTime utc, TimeZoneInfo zone)
    {
        var value = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(value, zone).ToString("dd MMM yyyy HH:mm");
    }

    public static string Format(DateTime? utc, TimeZoneInfo zone)
    {
        return utc.HasValue ? Format(utc.Value, zone) : "-";
    }
}

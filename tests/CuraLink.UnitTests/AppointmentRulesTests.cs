using CuraLink.Domain.Common;
using CuraLink.Domain.Enums;

namespace CuraLink.UnitTests;

public class AppointmentRulesTests
{
    private static readonly DateTime BaseUtc = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsValidRange_ReturnsTrueWhenEndIsAfterStart()
    {
        Assert.True(AppointmentRules.IsValidRange(BaseUtc, BaseUtc.AddMinutes(30)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsValidRange_ReturnsFalseWhenEndIsNotAfterStart(int endOffsetMinutes)
    {
        Assert.False(AppointmentRules.IsValidRange(BaseUtc, BaseUtc.AddMinutes(endOffsetMinutes)));
    }

    [Fact]
    public void IsInFuture_ReturnsTrueWhenStartIsAfterNow()
    {
        Assert.True(AppointmentRules.IsInFuture(BaseUtc.AddMinutes(1), BaseUtc));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsInFuture_ReturnsFalseWhenStartIsNowOrInThePast(int offsetMinutes)
    {
        Assert.False(AppointmentRules.IsInFuture(BaseUtc.AddMinutes(offsetMinutes), BaseUtc));
    }

    [Theory]
    [InlineData(0, 60, 30, 90, true)]
    [InlineData(0, 60, 10, 20, true)]
    [InlineData(0, 60, 60, 120, false)]
    [InlineData(0, 60, 70, 120, false)]
    [InlineData(0, 60, -30, 0, false)]
    public void Overlaps_ReturnsExpectedResult(int aStart, int aEnd, int bStart, int bEnd, bool expected)
    {
        var result = AppointmentRules.Overlaps(
            BaseUtc.AddMinutes(aStart),
            BaseUtc.AddMinutes(aEnd),
            BaseUtc.AddMinutes(bStart),
            BaseUtc.AddMinutes(bEnd));

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending, AppointmentStatus.Confirmed, true)]
    [InlineData(AppointmentStatus.Pending, AppointmentStatus.Rejected, true)]
    [InlineData(AppointmentStatus.Pending, AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Completed, true)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.NoShow, true)]
    [InlineData(AppointmentStatus.Pending, AppointmentStatus.Completed, false)]
    [InlineData(AppointmentStatus.Pending, AppointmentStatus.NoShow, false)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Rejected, false)]
    [InlineData(AppointmentStatus.Rejected, AppointmentStatus.Confirmed, false)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.Confirmed, false)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Confirmed, false)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.Confirmed, false)]
    [InlineData(AppointmentStatus.Pending, AppointmentStatus.Pending, false)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Confirmed, false)]
    public void IsStatusTransitionAllowed_ReturnsExpectedResult(AppointmentStatus current, AppointmentStatus next, bool expected)
    {
        Assert.Equal(expected, AppointmentRules.IsStatusTransitionAllowed(current, next));
    }
}

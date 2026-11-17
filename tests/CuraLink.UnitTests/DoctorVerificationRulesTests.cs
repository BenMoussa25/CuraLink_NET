using CuraLink.Domain.Common;
using CuraLink.Domain.Enums;

namespace CuraLink.UnitTests;

public class DoctorVerificationRulesTests
{
    [Theory]
    [InlineData(DoctorVerificationStatus.Approved, true, true, true)]
    [InlineData(DoctorVerificationStatus.Approved, false, true, false)]
    [InlineData(DoctorVerificationStatus.Approved, true, false, false)]
    [InlineData(DoctorVerificationStatus.Approved, false, false, false)]
    [InlineData(DoctorVerificationStatus.Pending, true, true, false)]
    [InlineData(DoctorVerificationStatus.Rejected, true, true, false)]
    [InlineData(DoctorVerificationStatus.Suspended, true, true, false)]
    public void CanBePubliclyListed_RequiresApprovedActiveDoctorAndSpecialty(
        DoctorVerificationStatus status,
        bool userIsActive,
        bool specialtyIsActive,
        bool expected)
    {
        Assert.Equal(expected, DoctorVerificationRules.CanBePubliclyListed(status, userIsActive, specialtyIsActive));
    }
}

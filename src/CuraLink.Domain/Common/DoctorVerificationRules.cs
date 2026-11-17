using CuraLink.Domain.Enums;

namespace CuraLink.Domain.Common;

public static class DoctorVerificationRules
{
    public static bool CanBePubliclyListed(DoctorVerificationStatus status, bool userIsActive, bool specialtyIsActive) =>
        status == DoctorVerificationStatus.Approved && userIsActive && specialtyIsActive;
}

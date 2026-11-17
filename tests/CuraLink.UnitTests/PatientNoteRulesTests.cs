using CuraLink.Domain.Common;
using CuraLink.Domain.Entities;

namespace CuraLink.UnitTests;

public class PatientNoteRulesTests
{
    [Fact]
    public void IsOwnedBy_ReturnsTrueWhenPatientUserIdMatches()
    {
        var note = CreateNote("patient-1");
        Assert.True(PatientNoteRules.IsOwnedBy(note, "patient-1"));
    }

    [Fact]
    public void IsOwnedBy_ReturnsFalseWhenPatientUserIdDiffers()
    {
        var note = CreateNote("patient-1");
        Assert.False(PatientNoteRules.IsOwnedBy(note, "patient-2"));
    }

    private static PatientNote CreateNote(string patientUserId)
    {
        return new PatientNote
        {
            Patient = new Patient { UserId = patientUserId },
            Title = "Follow-up",
            Content = "Review results"
        };
    }
}

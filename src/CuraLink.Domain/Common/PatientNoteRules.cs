using CuraLink.Domain.Entities;

namespace CuraLink.Domain.Common;

public static class PatientNoteRules
{
    public static bool IsOwnedBy(PatientNote note, string patientUserId) =>
        note?.Patient is not null && string.Equals(note.Patient.UserId, patientUserId, StringComparison.Ordinal);
}

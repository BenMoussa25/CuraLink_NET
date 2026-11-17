using CuraLink.Domain.Entities;

namespace CuraLink.Domain.Common;

public static class PrescriptionRules
{
    public static bool HasValidItems(IEnumerable<PrescriptionItem> items) =>
        items is not null
        && items.Any()
        && items.All(item => !string.IsNullOrWhiteSpace(item.MedicineName)
            && !string.IsNullOrWhiteSpace(item.DosageInstructions));
}

using CuraLink.Domain.Common;
using CuraLink.Domain.Entities;

namespace CuraLink.UnitTests;

public class PrescriptionRulesTests
{
    [Fact]
    public void HasValidItems_ReturnsFalseForEmptyCollection()
    {
        Assert.False(PrescriptionRules.HasValidItems(Array.Empty<PrescriptionItem>()));
    }

    [Fact]
    public void HasValidItems_ReturnsTrueForSingleValidItem()
    {
        Assert.True(PrescriptionRules.HasValidItems(new[] { CreateItem() }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HasValidItems_ReturnsFalseForBlankMedicineName(string medicineName)
    {
        var item = CreateItem(medicineName: medicineName);
        Assert.False(PrescriptionRules.HasValidItems(new[] { item }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HasValidItems_ReturnsFalseForBlankDosageInstructions(string dosageInstructions)
    {
        var item = CreateItem(dosageInstructions: dosageInstructions);
        Assert.False(PrescriptionRules.HasValidItems(new[] { item }));
    }

    [Fact]
    public void HasValidItems_ReturnsFalseWhenAnyItemIsInvalid()
    {
        var items = new[] { CreateItem(), CreateItem(medicineName: string.Empty) };
        Assert.False(PrescriptionRules.HasValidItems(items));
    }

    private static PrescriptionItem CreateItem(string medicineName = "Paracetamol", string dosageInstructions = "One tablet twice daily")
    {
        return new PrescriptionItem
        {
            MedicineName = medicineName,
            DosageInstructions = dosageInstructions
        };
    }
}

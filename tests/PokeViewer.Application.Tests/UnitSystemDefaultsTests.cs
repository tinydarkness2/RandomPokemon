using PokeViewer.Application.Units;

namespace PokeViewer.Application.Tests;

public class UnitSystemDefaultsTests
{
    [Theory]
    [InlineData("Imperial", "en-GB", UnitSystem.Imperial)]
    [InlineData("metric", "en-US", UnitSystem.Metric)]
    [InlineData(null, "en-US", UnitSystem.Imperial)]
    [InlineData(null, "EN-us", UnitSystem.Imperial)]
    [InlineData(null, "en-GB", UnitSystem.Metric)]
    [InlineData(null, "en", UnitSystem.Metric)]
    [InlineData(null, "de-DE", UnitSystem.Metric)]
    [InlineData(null, null, UnitSystem.Metric)]
    [InlineData("parsecs", "en-US", UnitSystem.Imperial)]
    [InlineData("7", "de-DE", UnitSystem.Metric)]
    public void SavedChoiceWins_ElseUsEnglishMeansImperial(string? saved, string? language, UnitSystem expected) =>
        Assert.Equal(expected, UnitSystemDefaults.Resolve(saved, language));
}

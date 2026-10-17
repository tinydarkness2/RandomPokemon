using PokeViewer.Application.Pokedex;
using PokeViewer.Application.Units;

namespace PokeViewer.Application.Tests;

public class MeasurementsTests
{
    private static PokemonDto Pokemon(decimal metres, int inches, decimal kilograms, decimal pounds) =>
        new(1, "Test", ["Normal"], metres, inches, kilograms, pounds, null,
            new MovesetDto("heartgold-soulsilver", "Heartgold Soulsilver", 0, []));

    [Fact]
    public void FormatsMetric()
    {
        var bulbasaur = Pokemon(0.7m, 28, 6.9m, 15.2m);

        Assert.Equal(new FormattedMeasurement("0.7 m", "0.7 metres"), Measurements.FormatHeight(bulbasaur, UnitSystem.Metric));
        Assert.Equal(new FormattedMeasurement("6.9 kg", "6.9 kilograms"), Measurements.FormatWeight(bulbasaur, UnitSystem.Metric));
    }

    [Fact]
    public void FormatsImperial_LikeThePokedex()
    {
        var bulbasaur = Pokemon(0.7m, 28, 6.9m, 15.2m);

        Assert.Equal(new FormattedMeasurement("2′04″", "2 feet 4 inches"), Measurements.FormatHeight(bulbasaur, UnitSystem.Imperial));
        Assert.Equal(new FormattedMeasurement("15.2 lb", "15.2 pounds"), Measurements.FormatWeight(bulbasaur, UnitSystem.Imperial));
    }

    [Theory]
    [InlineData(13, "1′01″", "1 foot 1 inch")]
    [InlineData(8, "0′08″", "0 feet 8 inches")]
    [InlineData(346, "28′10″", "28 feet 10 inches")]
    [InlineData(24, "2′00″", "2 feet 0 inches")]
    public void FormatsFeetAndInches(int inches, string text, string spoken) =>
        Assert.Equal(new FormattedMeasurement(text, spoken), Measurements.FormatHeight(Pokemon(0, inches, 0, 0), UnitSystem.Imperial));

    [Fact]
    public void FormatsWholeNumbersWithOneDecimal()
    {
        var onix = Pokemon(8.8m, 346, 210m, 463.0m);

        Assert.Equal("210.0 kg", Measurements.FormatWeight(onix, UnitSystem.Metric).Text);
        Assert.Equal("463.0 lb", Measurements.FormatWeight(onix, UnitSystem.Imperial).Text);
    }
}

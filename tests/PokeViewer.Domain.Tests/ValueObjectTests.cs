namespace PokeViewer.Domain.Tests;

public class ValueObjectTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PokemonId_RejectsNonPositiveValues(int value) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new PokemonId(value));

    [Fact]
    public void PokemonId_KeepsValue() => Assert.Equal(25, new PokemonId(25).Value);

    [Fact]
    public void Height_ConvertsDecimetresToMetres() => Assert.Equal(1.7m, new Height(17).Metres);

    [Fact]
    public void Weight_ConvertsHectogramsToKilograms() => Assert.Equal(90.5m, new Weight(905).Kilograms);

    [Fact]
    public void Height_RejectsNegative() => Assert.Throws<ArgumentOutOfRangeException>(() => new Height(-1));

    [Fact]
    public void Weight_RejectsNegative() => Assert.Throws<ArgumentOutOfRangeException>(() => new Weight(-1));

    [Fact]
    public void PokemonType_NormalisesToLowerCase() => Assert.Equal("grass", new PokemonType(" Grass ").Name);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PokemonType_RejectsBlankNames(string name) =>
        Assert.Throws<ArgumentException>(() => new PokemonType(name));

    [Fact]
    public void PokemonType_HasValueEquality() => Assert.Equal(new PokemonType("fire"), new PokemonType("FIRE"));
}

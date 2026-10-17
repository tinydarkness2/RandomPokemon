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

public class ImperialConversionTests
{
    // Expected values are what the Pokédex games show.
    [Theory]
    [InlineData(7, 28)]    // Bulbasaur 2′04″
    [InlineData(4, 16)]    // Pikachu 1′04″
    [InlineData(21, 83)]   // Snorlax 6′11″
    [InlineData(88, 346)]  // Onix 28′10″
    [InlineData(0, 0)]
    public void Height_ConvertsToWholeInches(int decimetres, int inches) =>
        Assert.Equal(inches, new Height(decimetres).TotalInches);

    [Theory]
    [InlineData(69, 15.2)]      // Bulbasaur
    [InlineData(60, 13.2)]      // Pikachu
    [InlineData(4600, 1014.1)]  // Snorlax
    [InlineData(2100, 463.0)]   // Onix
    [InlineData(1, 0.2)]        // Gastly
    public void Weight_ConvertsToPounds(int hectograms, double pounds) =>
        Assert.Equal((decimal)pounds, new Weight(hectograms).Pounds);
}

namespace PokeViewer.Domain.Tests;

public class PokemonTests
{
    private static readonly Moveset NoMoves = new("heartgold-soulsilver", []);

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var sprite = new Uri("https://example.test/1.png");

        var pokemon = new Pokemon(
            new PokemonId(1), "bulbasaur", [new PokemonType("grass"), new PokemonType("poison")],
            new Height(7), new Weight(69), sprite, NoMoves);

        Assert.Equal(1, pokemon.Id.Value);
        Assert.Equal("bulbasaur", pokemon.Name);
        Assert.Equal(["grass", "poison"], pokemon.Types.Select(t => t.Name));
        Assert.Equal(0.7m, pokemon.Height.Metres);
        Assert.Equal(6.9m, pokemon.Weight.Kilograms);
        Assert.Equal(sprite, pokemon.SpriteUrl);
        Assert.Same(NoMoves, pokemon.Moveset);
    }

    [Fact]
    public void Constructor_RequiresAtLeastOneType() =>
        Assert.Throws<ArgumentException>(() =>
            new Pokemon(new PokemonId(1), "bulbasaur", [], new Height(7), new Weight(69), null, NoMoves));

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_RequiresName(string name) =>
        Assert.Throws<ArgumentException>(() =>
            new Pokemon(new PokemonId(1), name, [new PokemonType("grass")], new Height(7), new Weight(69), null, NoMoves));

    [Fact]
    public void Types_CannotBeModifiedThroughTheSourceList()
    {
        var types = new List<PokemonType> { new("grass") };
        var pokemon = new Pokemon(new PokemonId(1), "bulbasaur", types, new Height(7), new Weight(69), null, NoMoves);

        types.Add(new PokemonType("fire"));

        Assert.Single(pokemon.Types);
    }
}

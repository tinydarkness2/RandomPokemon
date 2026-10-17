namespace PokeViewer.Domain.Tests;

public class MovesetTests
{
    [Fact]
    public void PokemonMove_NormalisesName() =>
        Assert.Equal("vine-whip", new PokemonMove(" Vine-Whip ", MoveLearnMethod.LevelUp, 9).Name);

    [Fact]
    public void PokemonMove_RejectsNegativeLevel() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new PokemonMove("tackle", MoveLearnMethod.LevelUp, -1));

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void PokemonMove_RejectsBlankName(string name) =>
        Assert.Throws<ArgumentException>(() => new PokemonMove(name, MoveLearnMethod.Egg));

    [Fact]
    public void Moveset_RemovesExactDuplicates()
    {
        var moveset = new Moveset("heartgold-soulsilver", [
            new PokemonMove("tackle", MoveLearnMethod.LevelUp, 1),
            new PokemonMove("tackle", MoveLearnMethod.LevelUp, 1),
            new PokemonMove("tackle", MoveLearnMethod.Tutor),
        ]);

        Assert.Equal(2, moveset.Moves.Count);
    }

    [Fact]
    public void Moveset_AllowsNoMoves() => Assert.Empty(new Moveset("platinum", []).Moves);

    [Fact]
    public void Moveset_RequiresVersionGroup() =>
        Assert.Throws<ArgumentException>(() => new Moveset(" ", []));
}

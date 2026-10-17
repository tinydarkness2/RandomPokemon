using PokeViewer.Application.Pokedex;
using PokeViewer.Domain;

namespace PokeViewer.Application.Tests;

public class MovesetMappingTests
{
    private static readonly Moveset Bulbasaur = new("heartgold-soulsilver", [
        new PokemonMove("vine-whip", MoveLearnMethod.LevelUp, 9),
        new PokemonMove("solar-beam", MoveLearnMethod.Machine),
        new PokemonMove("tackle", MoveLearnMethod.LevelUp, 1),
        new PokemonMove("growl", MoveLearnMethod.LevelUp, 3),
        new PokemonMove("petal-dance", MoveLearnMethod.Egg),
        new PokemonMove("cut", MoveLearnMethod.Machine),
        new PokemonMove("seed-bomb", MoveLearnMethod.Tutor),
        new PokemonMove("tackle", MoveLearnMethod.Tutor),
    ]);

    [Fact]
    public void GroupsInLearnMethodOrder()
    {
        var dto = Bulbasaur.ToDto();

        Assert.Equal(["Level up", "TM / HM", "Egg moves", "Move tutor"], dto.Groups.Select(g => g.Title));
    }

    [Fact]
    public void SortsLevelUpMovesByLevel()
    {
        var levelUp = Bulbasaur.ToDto().Groups[0];

        Assert.Equal(["Tackle", "Growl", "Vine Whip"], levelUp.Moves.Select(m => m.Name));
        Assert.Equal([1, 3, 9], levelUp.Moves.Select(m => m.Level));
    }

    [Fact]
    public void SortsOtherGroupsAlphabetically_WithoutLevels()
    {
        var machines = Bulbasaur.ToDto().Groups[1];

        Assert.Equal(["Cut", "Solar Beam"], machines.Moves.Select(m => m.Name));
        Assert.All(machines.Moves, m => Assert.Null(m.Level));
    }

    [Fact]
    public void CountsDistinctMoves()
    {
        // "tackle" is both a level-up and a tutor move but counts once.
        Assert.Equal(7, Bulbasaur.ToDto().MoveCount);
    }

    [Fact]
    public void UsesDisplayFriendlyGameName() =>
        Assert.Equal("Heartgold Soulsilver", Bulbasaur.ToDto().GameName);

    [Fact]
    public void OmitsEmptyGroups()
    {
        var dto = new Moveset("platinum", []).ToDto();

        Assert.Empty(dto.Groups);
        Assert.Equal(0, dto.MoveCount);
    }

    [Theory]
    [InlineData(MoveLearnMethod.LevelUp, "Level up")]
    [InlineData(MoveLearnMethod.Machine, "TM / HM")]
    [InlineData(MoveLearnMethod.Egg, "Egg moves")]
    [InlineData(MoveLearnMethod.Tutor, "Move tutor")]
    [InlineData(MoveLearnMethod.Other, "Other")]
    public void GroupTitle_IsFriendly(MoveLearnMethod method, string expected) =>
        Assert.Equal(expected, PokemonMapping.GroupTitle(method));
}

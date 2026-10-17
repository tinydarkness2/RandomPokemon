using Bunit;
using PokeViewer.Application.Pokedex;
using PokeViewer.Web.Components;

namespace PokeViewer.Web.Tests;

public sealed class PokemonMovesTests : BunitContext
{
    private static readonly MovesetDto Bulbasaur = new("Heartgold Soulsilver", 3,
    [
        new MoveGroupDto("Level up", [new MoveDto("Tackle", 1), new MoveDto("Vine Whip", 9)]),
        new MoveGroupDto("TM / HM", [new MoveDto("Solar Beam", null)]),
    ]);

    [Fact]
    public void IsCollapsedByDefault_AndShowsCount()
    {
        var moves = Render<PokemonMoves>(p => p.Add(c => c.Moveset, Bulbasaur));

        var details = moves.Find("details.moves");
        Assert.False(details.HasAttribute("open"));
        Assert.Equal("Moves (3)", details.QuerySelector("summary")!.TextContent);
    }

    [Fact]
    public void ListsGroupsInOrder_WithLevelsForLevelUpMoves()
    {
        var moves = Render<PokemonMoves>(p => p.Add(c => c.Moveset, Bulbasaur));

        Assert.Equal(["Level up", "TM / HM"], moves.FindAll(".move-group h3").Select(h => h.TextContent));
        Assert.Equal(["Lv. 1", "Lv. 9"], moves.FindAll(".level").Select(l => l.TextContent));
        Assert.Equal(["Tackle", "Vine Whip", "Solar Beam"], moves.FindAll(".move-name").Select(m => m.TextContent));
    }

    [Fact]
    public void ListIsScrollableRegion_AndNamesTheGame()
    {
        var moves = Render<PokemonMoves>(p => p.Add(c => c.Moveset, Bulbasaur));

        Assert.Equal("0", moves.Find(".move-list").GetAttribute("tabindex"));
        Assert.Equal("From Pokémon Heartgold Soulsilver", moves.Find(".source").TextContent);
    }

    [Fact]
    public void ShowsFriendlyMessage_WhenNoMoves()
    {
        var moves = Render<PokemonMoves>(p => p.Add(c => c.Moveset, new MovesetDto("Platinum", 0, [])));

        Assert.Equal("No moves recorded for Platinum.", moves.Find(".empty").TextContent);
        Assert.Empty(moves.FindAll(".move-list"));
    }
}

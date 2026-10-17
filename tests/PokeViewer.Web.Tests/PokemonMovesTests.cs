using AngleSharp.Dom;
using Bunit;
using PokeViewer.Application.Pokedex;
using PokeViewer.Web.Components;

namespace PokeViewer.Web.Tests;

public sealed class PokemonMovesTests : BunitContext
{
    private static readonly MovesetDto Bulbasaur = new("heartgold-soulsilver", "Heartgold Soulsilver", 3,
    [
        new MoveGroupDto("Level up", [new MoveDto("tackle", "Tackle", 1), new MoveDto("vine-whip", "Vine Whip", 9)]),
        new MoveGroupDto("TM / HM", [new MoveDto("solar-beam", "Solar Beam", null)]),
    ]);

    private readonly FakeMoveTypeRepository _moveTypes = new();

    public PokemonMovesTests()
    {
        _moveTypes.Types["tackle"] = "normal";
        _moveTypes.Types["vine-whip"] = "grass";
        Services.AddMoveTypeLookups(_moveTypes);
    }

    private IRenderedComponent<PokemonMoves> RenderMoves(MovesetDto? moveset = null) =>
        Render<PokemonMoves>(p => p.Add(c => c.Moveset, moveset ?? Bulbasaur));

    private static IElement Row(IRenderedComponent<PokemonMoves> moves, string name) =>
        moves.FindAll("li.move").Single(li => li.QuerySelector(".move-name")!.TextContent == name);

    [Fact]
    public void IsCollapsedByDefault_AndShowsCount()
    {
        var moves = RenderMoves();

        var details = moves.Find("details.moves");
        Assert.False(details.HasAttribute("open"));
        Assert.Equal("Moves (3)", details.QuerySelector("summary")!.TextContent);
    }

    [Fact]
    public void ListsGroupsInOrder_WithLevelsForLevelUpMoves()
    {
        var moves = RenderMoves();

        Assert.Equal(["Level up", "TM / HM"], moves.FindAll(".move-group h3").Select(h => h.TextContent));
        Assert.Equal(["Lv. 1", "Lv. 9"], moves.FindAll(".level").Select(l => l.TextContent));
        Assert.Equal(["Tackle", "Vine Whip", "Solar Beam"], moves.FindAll(".move-name").Select(m => m.TextContent));
    }

    [Fact]
    public void ListIsScrollableRegion_AndNamesTheGame()
    {
        var moves = RenderMoves();

        Assert.Equal("0", moves.Find(".move-list").GetAttribute("tabindex"));
        Assert.Contains("From Pokémon Heartgold Soulsilver", moves.Find(".source").TextContent);
    }

    [Fact]
    public void ShowsFriendlyMessage_WhenNoMoves()
    {
        var moves = RenderMoves(new MovesetDto("platinum", "Platinum", 0, []));

        Assert.Equal("No moves recorded for Platinum.", moves.Find(".empty").TextContent);
        Assert.Empty(moves.FindAll(".move-list"));
    }

    [Fact]
    public void DoesNotLookUpTypes_UntilOpened()
    {
        RenderMoves();

        Assert.Empty(_moveTypes.Requests);
    }

    [Fact]
    public void Opening_TintsEachRowWithItsMoveType()
    {
        var moves = RenderMoves();

        moves.Find("details").Toggle();

        moves.WaitForAssertion(() => Assert.Equal("Grass", Row(moves, "Vine Whip").QuerySelector(".move-type")!.TextContent));
        var vineWhip = Row(moves, "Vine Whip");
        Assert.Contains("typed", vineWhip.ClassList);
        Assert.Equal("--type-color: var(--type-grass)", vineWhip.GetAttribute("style"));
        Assert.Equal("Normal", Row(moves, "Tackle").QuerySelector(".move-type")!.TextContent);
        Assert.True(moves.Find("details").HasAttribute("open"));
        Assert.All(_moveTypes.Requests, r => Assert.Equal("heartgold-soulsilver", r.VersionGroup));
    }

    [Fact]
    public void ShowsUnknownType_AsQuestionMarks()
    {
        _moveTypes.Types["solar-beam"] = "unknown";
        var moves = RenderMoves();

        moves.Find("details").Toggle();

        moves.WaitForAssertion(() => Assert.Equal("???", Row(moves, "Solar Beam").QuerySelector(".move-type")!.TextContent));
        Assert.Equal("--type-color: var(--type-unknown)", Row(moves, "Solar Beam").GetAttribute("style"));
    }

    [Fact]
    public void LeavesRowUncoloured_WhenItsTypeCannotBeLoaded()
    {
        var moves = RenderMoves();

        moves.Find("details").Toggle();

        moves.WaitForAssertion(() => Assert.NotNull(Row(moves, "Tackle").QuerySelector(".move-type")));
        var solarBeam = Row(moves, "Solar Beam");
        Assert.DoesNotContain("typed", solarBeam.ClassList);
        Assert.Null(solarBeam.GetAttribute("style"));
        Assert.Null(solarBeam.QuerySelector(".move-type"));
    }

    [Fact]
    public void ShowsLoadingNote_WhileTypesLoad()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _moveTypes.Gate = gate.Task;
        var moves = RenderMoves();

        moves.Find("details").Toggle();
        Assert.NotNull(moves.Find(".loading-types"));

        gate.SetResult();
        moves.WaitForAssertion(() => Assert.Empty(moves.FindAll(".loading-types")));
    }

    [Fact]
    public void LooksUpTypesOnlyOnce_WhenReopened()
    {
        var moves = RenderMoves();
        var details = moves.Find("details");

        details.Toggle();
        moves.WaitForAssertion(() => Assert.NotNull(Row(moves, "Tackle").QuerySelector(".move-type")));
        var requestsAfterFirstOpen = _moveTypes.Requests.Count;

        moves.Find("details").Toggle(); // close
        moves.Find("details").Toggle(); // open again

        Assert.Equal(3, requestsAfterFirstOpen);
        Assert.Equal(requestsAfterFirstOpen, _moveTypes.Requests.Count);
    }

    [Fact]
    public void LoadsTypesForNextPokemon_WhenLeftOpen()
    {
        _moveTypes.Types["thunder-shock"] = "electric";
        var moves = RenderMoves();
        moves.Find("details").Toggle();
        moves.WaitForAssertion(() => Assert.NotNull(Row(moves, "Tackle").QuerySelector(".move-type")));

        moves.Render(p => p.Add(c => c.Moveset, new MovesetDto("heartgold-soulsilver", "Heartgold Soulsilver", 1,
            [new MoveGroupDto("Level up", [new MoveDto("thunder-shock", "Thunder Shock", 1)])])));

        moves.WaitForAssertion(() => Assert.Equal("Electric", Row(moves, "Thunder Shock").QuerySelector(".move-type")!.TextContent));
    }
}

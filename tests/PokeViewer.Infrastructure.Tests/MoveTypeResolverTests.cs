using PokeViewer.Infrastructure.PokeApi;

namespace PokeViewer.Infrastructure.Tests;

public class MoveTypeResolverTests
{
    // Real PokeAPI version group orders.
    private static readonly Dictionary<string, int> Orders = new()
    {
        ["red-blue"] = 1,
        ["gold-silver"] = 3,
        ["heartgold-soulsilver"] = 10,
        ["x-y"] = 15,
    };

    private static PokeApiMoveResponse Move(string currentType, params (string? Type, string VersionGroup)[] pastValues) =>
        new("some-move", new PokeApiNamedResource(currentType),
            pastValues.Select(p => new PokeApiMovePastValue(
                p.Type is null ? null : new PokeApiNamedResource(p.Type),
                new PokeApiNamedResource(p.VersionGroup))).ToList());

    [Fact]
    public void UsesCurrentType_WhenTypeNeverChanged() =>
        Assert.Equal("grass", MoveTypeResolver.Resolve(Move("grass"), "heartgold-soulsilver", Orders));

    [Fact]
    public void UsesOldType_WhenTypeChangedInALaterGame()
    {
        // Charm became Fairy in X/Y; it was Normal in HeartGold/SoulSilver.
        var charm = Move("fairy", ("normal", "x-y"));

        Assert.Equal("normal", MoveTypeResolver.Resolve(charm, "heartgold-soulsilver", Orders));
    }

    [Fact]
    public void UsesCurrentType_WhenTypeChangedInAnEarlierGame()
    {
        // Bite became Dark in Gold/Silver; it was Normal in Red/Blue.
        var bite = Move("dark", ("normal", "gold-silver"));

        Assert.Equal("dark", MoveTypeResolver.Resolve(bite, "heartgold-soulsilver", Orders));
        Assert.Equal("normal", MoveTypeResolver.Resolve(bite, "red-blue", Orders));
    }

    [Fact]
    public void PicksTheFirstLaterChange_WhenTypeChangedMoreThanOnce()
    {
        var move = Move("fairy", ("normal", "gold-silver"), ("psychic", "x-y"));

        Assert.Equal("normal", MoveTypeResolver.Resolve(move, "red-blue", Orders));
        Assert.Equal("psychic", MoveTypeResolver.Resolve(move, "heartgold-soulsilver", Orders));
        Assert.Equal("fairy", MoveTypeResolver.Resolve(move, "x-y", Orders));
    }

    [Fact]
    public void IgnoresPastValuesThatDidNotChangeTheType()
    {
        var move = Move("fairy", (null, "gold-silver"), ("normal", "x-y"));

        Assert.Equal("normal", MoveTypeResolver.Resolve(move, "red-blue", Orders));
        Assert.Equal(["x-y", "red-blue"], MoveTypeResolver.VersionGroupsNeeded(move, "red-blue"));
    }

    [Fact]
    public void NeedsNoVersionGroupOrders_WhenTypeNeverChanged() =>
        Assert.Empty(MoveTypeResolver.VersionGroupsNeeded(Move("grass", (null, "x-y")), "heartgold-soulsilver"));
}

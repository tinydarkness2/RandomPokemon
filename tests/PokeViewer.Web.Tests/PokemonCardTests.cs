using Bunit;
using PokeViewer.Application.Pokedex;
using PokeViewer.Web.Components;

namespace PokeViewer.Web.Tests;

public sealed class PokemonCardTests : BunitContext
{
    private static readonly PokemonDto Bulbasaur =
        new(1, "Bulbasaur", ["Grass", "Poison"], 0.7m, 6.9m, "https://img.example.test/1.png",
            new MovesetDto("heartgold-soulsilver", "Heartgold Soulsilver", 1,
                [new MoveGroupDto("Level up", [new MoveDto("tackle", "Tackle", 1)])]));

    public PokemonCardTests() => Services.AddMoveTypeLookups(new FakeMoveTypeRepository());

    [Fact]
    public void ShowsNameNumberAndTypes()
    {
        var card = Render<PokemonCard>(p => p.Add(c => c.Pokemon, Bulbasaur));

        Assert.Equal("#001", card.Find(".number").TextContent);
        Assert.Equal("Bulbasaur", card.Find(".name").TextContent);
        Assert.Equal(["Grass", "Poison"], card.FindAll(".type").Select(t => t.TextContent));
        Assert.Contains("type-grass", card.Find(".type").ClassList);
    }

    [Fact]
    public void ShowsMetricHeightAndWeight()
    {
        var card = Render<PokemonCard>(p => p.Add(c => c.Pokemon, Bulbasaur));

        Assert.Equal("0.7 m", card.Find(".height").TextContent);
        Assert.Equal("6.9 kg", card.Find(".weight").TextContent);
    }

    [Fact]
    public void ShowsSpriteWithAltText()
    {
        var card = Render<PokemonCard>(p => p.Add(c => c.Pokemon, Bulbasaur));

        var img = card.Find("img.sprite");
        Assert.Equal("https://img.example.test/1.png", img.GetAttribute("src"));
        Assert.Equal("Bulbasaur", img.GetAttribute("alt"));
    }

    [Fact]
    public void ShowsPlaceholder_WhenNoSprite()
    {
        var card = Render<PokemonCard>(p => p.Add(c => c.Pokemon, Bulbasaur with { SpriteUrl = null }));

        Assert.Empty(card.FindAll("img"));
        Assert.NotNull(card.Find(".sprite.missing"));
    }

    [Fact]
    public void IncludesMovesSection()
    {
        var card = Render<PokemonCard>(p => p.Add(c => c.Pokemon, Bulbasaur));

        Assert.Equal("Moves (1)", card.Find(".moves summary").TextContent);
    }
}

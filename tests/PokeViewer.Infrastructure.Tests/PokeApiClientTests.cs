using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Domain;
using PokeViewer.Infrastructure.PokeApi;

namespace PokeViewer.Infrastructure.Tests;

public class PokeApiClientTests
{
    private static PokeApiClient CreateClient(HttpMessageHandler handler, string moveVersionGroup = "heartgold-soulsilver") =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://pokeapi.test/api/v2/") },
            Options.Create(new PokeApiOptions { MoveVersionGroup = moveVersionGroup }),
            NullLogger<PokeApiClient>.Instance);

    [Fact]
    public async Task GetPokemonAsync_RequestsThePokemonEndpoint()
    {
        var handler = FakeHttpMessageHandler.Json(PokeApiSamples.Bulbasaur);

        await CreateClient(handler).GetPokemonAsync(new PokemonId(1), CancellationToken.None);

        Assert.Equal(new Uri("https://pokeapi.test/api/v2/pokemon/1"), Assert.Single(handler.RequestedUris));
    }

    [Fact]
    public async Task GetPokemonAsync_MapsResponseToDomain()
    {
        var pokemon = await CreateClient(FakeHttpMessageHandler.Json(PokeApiSamples.Bulbasaur))
            .GetPokemonAsync(new PokemonId(1), CancellationToken.None);

        Assert.Equal(1, pokemon.Id.Value);
        Assert.Equal("bulbasaur", pokemon.Name);
        Assert.Equal(["grass", "poison"], pokemon.Types.Select(t => t.Name)); // ordered by slot
        Assert.Equal(7, pokemon.Height.Decimetres);
        Assert.Equal(69, pokemon.Weight.Hectograms);
        Assert.Equal(new Uri("https://img.example.test/artwork/1.png"), pokemon.SpriteUrl);
    }

    [Fact]
    public async Task GetPokemonAsync_MapsMovesFromConfiguredGameOnly()
    {
        var pokemon = await CreateClient(FakeHttpMessageHandler.Json(PokeApiSamples.Bulbasaur))
            .GetPokemonAsync(new PokemonId(1), CancellationToken.None);

        Assert.Equal("heartgold-soulsilver", pokemon.Moveset.VersionGroup);
        Assert.Equal(
            [
                new PokemonMove("vine-whip", MoveLearnMethod.LevelUp, 9),
                new PokemonMove("solar-beam", MoveLearnMethod.Machine),
                new PokemonMove("petal-dance", MoveLearnMethod.Egg),
                new PokemonMove("seed-bomb", MoveLearnMethod.Tutor),
                new PokemonMove("seed-bomb", MoveLearnMethod.Other),
            ],
            pokemon.Moveset.Moves);
    }

    [Fact]
    public async Task GetPokemonAsync_UsesMoveVersionGroupFromOptions()
    {
        var pokemon = await CreateClient(FakeHttpMessageHandler.Json(PokeApiSamples.Bulbasaur), "scarlet-violet")
            .GetPokemonAsync(new PokemonId(1), CancellationToken.None);

        Assert.Equal(
            [new PokemonMove("vine-whip", MoveLearnMethod.LevelUp, 3), new PokemonMove("leaf-storm", MoveLearnMethod.Tutor)],
            pokemon.Moveset.Moves);
    }

    [Fact]
    public async Task GetPokemonAsync_AllowsMissingMoves()
    {
        var pokemon = await CreateClient(FakeHttpMessageHandler.Json(PokeApiSamples.NoSprites))
            .GetPokemonAsync(new PokemonId(201), CancellationToken.None);

        Assert.Empty(pokemon.Moveset.Moves);
    }

    [Fact]
    public async Task GetPokemonAsync_FallsBackToDefaultSprite_WhenNoArtwork()
    {
        var pokemon = await CreateClient(FakeHttpMessageHandler.Json(PokeApiSamples.NoArtwork))
            .GetPokemonAsync(new PokemonId(132), CancellationToken.None);

        Assert.Equal(new Uri("https://img.example.test/sprites/132.png"), pokemon.SpriteUrl);
    }

    [Fact]
    public async Task GetPokemonAsync_AllowsMissingSprites()
    {
        var pokemon = await CreateClient(FakeHttpMessageHandler.Json(PokeApiSamples.NoSprites))
            .GetPokemonAsync(new PokemonId(201), CancellationToken.None);

        Assert.Null(pokemon.SpriteUrl);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetPokemonAsync_ThrowsUnavailable_OnErrorStatus(HttpStatusCode status)
    {
        var client = CreateClient(FakeHttpMessageHandler.Status(status));

        await Assert.ThrowsAsync<PokemonUnavailableException>(
            () => client.GetPokemonAsync(new PokemonId(1), CancellationToken.None));
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("""{ "id": 1, "name": "bulbasaur", "height": 7, "weight": 69, "types": [] }""")]
    public async Task GetPokemonAsync_ThrowsUnavailable_OnUnusableBody(string body)
    {
        var client = CreateClient(FakeHttpMessageHandler.Json(body));

        await Assert.ThrowsAsync<PokemonUnavailableException>(
            () => client.GetPokemonAsync(new PokemonId(1), CancellationToken.None));
    }

    [Fact]
    public async Task GetPokemonAsync_ThrowsUnavailable_WhenNetworkFails()
    {
        var client = CreateClient(new FakeHttpMessageHandler(_ => throw new HttpRequestException("offline")));

        await Assert.ThrowsAsync<PokemonUnavailableException>(
            () => client.GetPokemonAsync(new PokemonId(1), CancellationToken.None));
    }

    [Fact]
    public async Task GetPokemonAsync_LetsCallerCancellationThrough()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var client = CreateClient(FakeHttpMessageHandler.Json(PokeApiSamples.Bulbasaur));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetPokemonAsync(new PokemonId(1), cts.Token));
    }
}

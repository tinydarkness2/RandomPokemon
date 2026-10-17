using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Infrastructure.PokeApi;

namespace PokeViewer.Infrastructure.Tests;

public sealed class CachedMoveTypeRepositoryTests : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    private const string VineWhip = """
        { "name": "vine-whip", "type": { "name": "grass" }, "power": 45, "past_values": [] }
        """;

    private const string Charm = """
        { "name": "charm", "type": { "name": "fairy" },
          "past_values": [ { "type": { "name": "normal" }, "power": null, "version_group": { "name": "x-y" } } ] }
        """;

    private static FakeHttpMessageHandler Api() => new(request => request.RequestUri!.AbsolutePath switch
    {
        "/api/v2/move/vine-whip" => Json(VineWhip),
        "/api/v2/move/charm" => Json(Charm),
        "/api/v2/version-group/heartgold-soulsilver" => Json("""{ "name": "heartgold-soulsilver", "order": 10 }"""),
        "/api/v2/version-group/x-y" => Json("""{ "name": "x-y", "order": 15 }"""),
        _ => new HttpResponseMessage(HttpStatusCode.NotFound),
    });

    private static HttpResponseMessage Json(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private CachedMoveTypeRepository CreateRepository(HttpMessageHandler handler) => new(
        new PokeApiClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://pokeapi.test/api/v2/") },
            Options.Create(new PokeApiOptions()),
            NullLogger<PokeApiClient>.Instance),
        _cache,
        Options.Create(new PokeApiOptions()));

    [Fact]
    public async Task ReturnsCurrentType_WithoutVersionGroupLookups_WhenTypeNeverChanged()
    {
        var handler = Api();

        var type = await CreateRepository(handler).GetMoveTypeAsync("vine-whip", "heartgold-soulsilver", CancellationToken.None);

        Assert.Equal("grass", type.Name);
        Assert.Equal(["/api/v2/move/vine-whip"], handler.RequestedUris.Select(u => u.AbsolutePath));
    }

    [Fact]
    public async Task ReturnsTypeAsItWasInTheConfiguredGame()
    {
        var type = await CreateRepository(Api()).GetMoveTypeAsync("charm", "heartgold-soulsilver", CancellationToken.None);

        Assert.Equal("normal", type.Name);
    }

    [Fact]
    public async Task CachesMoveTypesAndVersionGroupOrders()
    {
        var handler = Api();
        var repository = CreateRepository(handler);

        await repository.GetMoveTypeAsync("charm", "heartgold-soulsilver", CancellationToken.None);
        await repository.GetMoveTypeAsync("charm", "heartgold-soulsilver", CancellationToken.None);

        Assert.Equal(3, handler.RequestedUris.Count); // move + two version groups, once each
    }

    [Fact]
    public async Task ThrowsUnavailable_WhenMoveIsUnknown()
    {
        await Assert.ThrowsAsync<PokemonUnavailableException>(
            () => CreateRepository(Api()).GetMoveTypeAsync("not-a-move", "heartgold-soulsilver", CancellationToken.None));
    }

    [Fact]
    public async Task EscapesMoveNamesInTheUrl()
    {
        var handler = Api();

        await Assert.ThrowsAsync<PokemonUnavailableException>(
            () => CreateRepository(handler).GetMoveTypeAsync("../pokemon/1", "heartgold-soulsilver", CancellationToken.None));

        Assert.Equal("/api/v2/move/..%2Fpokemon%2F1", Assert.Single(handler.RequestedUris).AbsolutePath);
    }

    public void Dispose() => _cache.Dispose();
}

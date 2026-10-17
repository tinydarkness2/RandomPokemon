using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Domain;
using PokeViewer.Infrastructure.PokeApi;

namespace PokeViewer.Infrastructure.Tests;

public sealed class CachedPokemonRepositoryTests : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    private CachedPokemonRepository CreateRepository(HttpMessageHandler handler) => new(
        new PokeApiClient(
            new HttpClient(handler) { BaseAddress = new Uri("https://pokeapi.test/api/v2/") },
            Options.Create(new PokeApiOptions()),
            NullLogger<PokeApiClient>.Instance),
        _cache,
        Options.Create(new PokeApiOptions()));

    [Fact]
    public async Task GetByIdAsync_ServesRepeatRequestsFromCache()
    {
        var handler = FakeHttpMessageHandler.Json(PokeApiSamples.Bulbasaur);
        var repository = CreateRepository(handler);

        var first = await repository.GetByIdAsync(new PokemonId(1), CancellationToken.None);
        var second = await repository.GetByIdAsync(new PokemonId(1), CancellationToken.None);

        Assert.Single(handler.RequestedUris);
        Assert.Same(first, second);
    }

    [Fact]
    public async Task GetByIdAsync_DoesNotCacheFailures()
    {
        var calls = 0;
        var handler = new FakeHttpMessageHandler(_ => ++calls == 1
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(PokeApiSamples.Bulbasaur, Encoding.UTF8, "application/json"),
            });
        var repository = CreateRepository(handler);

        await Assert.ThrowsAsync<PokemonUnavailableException>(
            () => repository.GetByIdAsync(new PokemonId(1), CancellationToken.None));
        var pokemon = await repository.GetByIdAsync(new PokemonId(1), CancellationToken.None);

        Assert.Equal("bulbasaur", pokemon.Name);
        Assert.Equal(2, handler.RequestedUris.Count);
    }

    public void Dispose() => _cache.Dispose();
}

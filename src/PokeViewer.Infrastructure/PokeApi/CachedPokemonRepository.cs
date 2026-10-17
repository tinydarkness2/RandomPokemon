using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Domain;

namespace PokeViewer.Infrastructure.PokeApi;

/// <summary>Serves Pokémon from memory when possible, falling back to PokeAPI.</summary>
internal sealed class CachedPokemonRepository(
    PokeApiClient client,
    IMemoryCache cache,
    IOptions<PokeApiOptions> options) : IPokemonRepository
{
    public async Task<Pokemon> GetByIdAsync(PokemonId id, CancellationToken cancellationToken)
    {
        var key = $"pokeapi:pokemon:{id}";

        if (cache.TryGetValue(key, out Pokemon? cached) && cached is not null)
        {
            return cached;
        }

        // Failures throw before reaching Set, so they are never cached.
        var pokemon = await client.GetPokemonAsync(id, cancellationToken);
        cache.Set(key, pokemon, options.Value.CacheDuration);
        return pokemon;
    }
}

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Domain;

namespace PokeViewer.Infrastructure.PokeApi;

/// <summary>
/// Looks up move types via PokeAPI, caching each answer app-wide. Gen 1–4 only has
/// ~470 moves, so after a little use almost every lookup is served from memory.
/// </summary>
internal sealed class CachedMoveTypeRepository(
    PokeApiClient client,
    IMemoryCache cache,
    IOptions<PokeApiOptions> options) : IMoveTypeRepository
{
    public async Task<PokemonType> GetMoveTypeAsync(string moveName, string versionGroup, CancellationToken cancellationToken)
    {
        var key = $"pokeapi:move-type:{versionGroup}:{moveName}";
        if (cache.TryGetValue(key, out PokemonType? cached) && cached is not null)
        {
            return cached;
        }

        var move = await client.GetMoveAsync(moveName, cancellationToken);

        var orders = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var group in MoveTypeResolver.VersionGroupsNeeded(move, versionGroup))
        {
            orders[group] = await GetVersionGroupOrderAsync(group, cancellationToken);
        }

        var type = new PokemonType(MoveTypeResolver.Resolve(move, versionGroup, orders));
        cache.Set(key, type, options.Value.CacheDuration);
        return type;
    }

    private async Task<int> GetVersionGroupOrderAsync(string versionGroup, CancellationToken cancellationToken)
    {
        var key = $"pokeapi:version-group-order:{versionGroup}";
        if (cache.TryGetValue(key, out int order))
        {
            return order;
        }

        order = await client.GetVersionGroupOrderAsync(versionGroup, cancellationToken);
        cache.Set(key, order, options.Value.CacheDuration);
        return order;
    }
}

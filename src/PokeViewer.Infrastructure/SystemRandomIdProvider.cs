using PokeViewer.Application.Abstractions;

namespace PokeViewer.Infrastructure;

internal sealed class SystemRandomIdProvider : IRandomIdProvider
{
#pragma warning disable CA5394 // Not security sensitive: picks which Pokémon to show.
    public int NextInRange(int minInclusive, int maxInclusive) => Random.Shared.Next(minInclusive, maxInclusive + 1);
#pragma warning restore CA5394
}

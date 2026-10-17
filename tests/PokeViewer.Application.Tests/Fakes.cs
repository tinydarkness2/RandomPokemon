using PokeViewer.Application.Abstractions;
using PokeViewer.Domain;

namespace PokeViewer.Application.Tests;

internal sealed class FakePokemonRepository : IPokemonRepository
{
    public List<PokemonId> RequestedIds { get; } = [];

    public Func<PokemonId, Pokemon> Respond { get; set; } = Sample;

    public Task<Pokemon> GetByIdAsync(PokemonId id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RequestedIds.Add(id);
        return Task.FromResult(Respond(id));
    }

    public static Pokemon Sample(PokemonId id) => new(
        id, "mr-mime", [new PokemonType("psychic"), new PokemonType("fairy")],
        new Height(13), new Weight(545), new Uri($"https://example.test/{id}.png"),
        new Moveset("heartgold-soulsilver", [
            new PokemonMove("confusion", MoveLearnMethod.LevelUp, 1),
            new PokemonMove("psychic", MoveLearnMethod.Machine),
        ]));
}

internal sealed class FakeRandomIdProvider(int value) : IRandomIdProvider
{
    public (int Min, int Max)? LastRange { get; private set; }

    public int NextInRange(int minInclusive, int maxInclusive)
    {
        LastRange = (minInclusive, maxInclusive);
        return value;
    }
}

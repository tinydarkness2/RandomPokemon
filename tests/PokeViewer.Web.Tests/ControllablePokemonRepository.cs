using PokeViewer.Application.Abstractions;
using PokeViewer.Domain;

namespace PokeViewer.Web.Tests;

/// <summary>A repository whose responses the test completes by hand, so loading states can be observed.</summary>
internal sealed class ControllablePokemonRepository : IPokemonRepository
{
    private TaskCompletionSource<Pokemon> _pending = NewPending();

    public CancellationToken LastToken { get; private set; }

    public Task<Pokemon> GetByIdAsync(PokemonId id, CancellationToken cancellationToken)
    {
        LastToken = cancellationToken;
        _pending = NewPending();
        cancellationToken.Register(() => _pending.TrySetCanceled(cancellationToken));
        return _pending.Task;
    }

    public void Succeed(Pokemon pokemon) => _pending.SetResult(pokemon);

    public void Fail() => _pending.SetException(new PokemonUnavailableException("PokeAPI is resting."));

    public static Pokemon Pikachu() => new(
        new PokemonId(25), "pikachu", [new PokemonType("electric")],
        new Height(4), new Weight(60), new Uri("https://img.example.test/25.png"));

    private static TaskCompletionSource<Pokemon> NewPending() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

internal sealed class FixedRandomIdProvider : IRandomIdProvider
{
    public int NextInRange(int minInclusive, int maxInclusive) => 25;
}

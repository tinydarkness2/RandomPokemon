using Microsoft.Extensions.Logging.Abstractions;
using PokeViewer.Application.Abstractions;
using PokeViewer.Application.Pokedex;
using PokeViewer.Domain;

namespace PokeViewer.Application.Tests;

public class GetMoveTypesHandlerTests
{
    private static readonly MovesetDto Moveset = new("heartgold-soulsilver", "Heartgold Soulsilver", 3,
    [
        new MoveGroupDto("Level up", [new MoveDto("tackle", "Tackle", 1), new MoveDto("vine-whip", "Vine Whip", 9)]),
        new MoveGroupDto("Move tutor", [new MoveDto("tackle", "Tackle", null), new MoveDto("charm", "Charm", null)]),
    ]);

    private readonly FakeMoveTypeRepository _repository = new()
    {
        Types = { ["tackle"] = "normal", ["vine-whip"] = "grass", ["charm"] = "normal" },
    };

    private GetMoveTypesHandler CreateHandler() => new(_repository, NullLogger<GetMoveTypesHandler>.Instance);

    [Fact]
    public async Task ReturnsDisplayTypeNamesByMoveKey()
    {
        var types = await CreateHandler().HandleAsync(Moveset, CancellationToken.None);

        Assert.Equal(new MoveTypeDto("normal", "Normal"), types["tackle"]);
        Assert.Equal(new MoveTypeDto("grass", "Grass"), types["vine-whip"]);
    }

    [Fact]
    public async Task ShowsUnknownTypeAsQuestionMarks()
    {
        // Curse had the "???" type in Gen 2–4.
        _repository.Types["charm"] = "unknown";

        var types = await CreateHandler().HandleAsync(Moveset, CancellationToken.None);

        Assert.Equal(new MoveTypeDto("unknown", "???"), types["charm"]);
    }

    [Fact]
    public async Task LooksUpEachMoveOnce_ForTheMovesetsGame()
    {
        await CreateHandler().HandleAsync(Moveset, CancellationToken.None);

        Assert.Equal(["charm", "tackle", "vine-whip"], _repository.Requests.Select(r => r.Move).Order());
        Assert.All(_repository.Requests, r => Assert.Equal("heartgold-soulsilver", r.VersionGroup));
    }

    [Fact]
    public async Task SkipsMovesWhoseTypeIsUnavailable()
    {
        _repository.Types.Remove("charm");

        var types = await CreateHandler().HandleAsync(Moveset, CancellationToken.None);

        Assert.False(types.ContainsKey("charm"));
        Assert.Equal(2, types.Count);
    }

    [Fact]
    public async Task LimitsConcurrentLookups()
    {
        var moves = Enumerable.Range(1, 30).Select(i => new MoveDto($"move-{i}", $"Move {i}", null)).ToList();
        foreach (var move in moves)
        {
            _repository.Types[move.Key] = "normal";
        }
        _repository.Delay = TimeSpan.FromMilliseconds(10);

        await CreateHandler().HandleAsync(
            new MovesetDto("heartgold-soulsilver", "Heartgold Soulsilver", 30, [new MoveGroupDto("Egg moves", moves)]),
            CancellationToken.None);

        Assert.InRange(_repository.MaxConcurrency, 1, GetMoveTypesHandler.MaxConcurrentLookups);
    }

    [Fact]
    public async Task PassesCancellationThrough()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateHandler().HandleAsync(Moveset, cts.Token));
    }

    private sealed class FakeMoveTypeRepository : IMoveTypeRepository
    {
        private int _inFlight;

        public Dictionary<string, string> Types { get; init; } = [];

        public List<(string Move, string VersionGroup)> Requests { get; } = [];

        public TimeSpan Delay { get; set; }

        public int MaxConcurrency { get; private set; }

        public async Task<PokemonType> GetMoveTypeAsync(string moveName, string versionGroup, CancellationToken cancellationToken)
        {
            var inFlight = Interlocked.Increment(ref _inFlight);
            lock (Requests)
            {
                Requests.Add((moveName, versionGroup));
                MaxConcurrency = Math.Max(MaxConcurrency, inFlight);
            }

            try
            {
                await Task.Delay(Delay, cancellationToken);
                return Types.TryGetValue(moveName, out var type)
                    ? new PokemonType(type)
                    : throw new PokemonUnavailableException(moveName);
            }
            finally
            {
                Interlocked.Decrement(ref _inFlight);
            }
        }
    }
}

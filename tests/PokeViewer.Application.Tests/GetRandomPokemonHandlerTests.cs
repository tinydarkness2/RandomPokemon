using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Application.Pokedex;

namespace PokeViewer.Application.Tests;

public class GetRandomPokemonHandlerTests
{
    private readonly FakePokemonRepository _repository = new();
    private readonly FakeRandomIdProvider _random = new(122);

    private GetRandomPokemonHandler CreateHandler(int minId = 1, int maxId = 493) => new(
        _repository,
        _random,
        Options.Create(new PokedexOptions { MinId = minId, MaxId = maxId }),
        NullLogger<GetRandomPokemonHandler>.Instance);

    [Fact]
    public async Task HandleAsync_UsesConfiguredRange()
    {
        await CreateHandler(minId: 1, maxId: 493).HandleAsync(CancellationToken.None);

        Assert.Equal((1, 493), _random.LastRange);
    }

    [Fact]
    public async Task HandleAsync_FetchesTheRandomlyChosenId()
    {
        await CreateHandler().HandleAsync(CancellationToken.None);

        Assert.Equal(122, Assert.Single(_repository.RequestedIds).Value);
    }

    [Fact]
    public async Task HandleAsync_MapsPokemonToDisplayFriendlyDto()
    {
        var result = await CreateHandler().HandleAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Pokemon;
        Assert.Equal(122, dto.Id);
        Assert.Equal("Mr Mime", dto.Name);
        Assert.Equal(["Psychic", "Fairy"], dto.Types);
        Assert.Equal(1.3m, dto.HeightMetres);
        Assert.Equal(54.5m, dto.WeightKilograms);
        Assert.Equal("https://example.test/122.png", dto.SpriteUrl);
    }

    [Fact]
    public async Task HandleAsync_ReturnsFriendlyFailure_WhenPokemonIsUnavailable()
    {
        _repository.Respond = _ => throw new PokemonUnavailableException("boom");

        var result = await CreateHandler().HandleAsync(CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(GetRandomPokemonHandler.FriendlyErrorMessage, result.ErrorMessage);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationThrough()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateHandler().HandleAsync(cts.Token));
    }
}

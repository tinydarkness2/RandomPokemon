using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Application.Pokedex;
using PokeViewer.Web.Components.Pages;

namespace PokeViewer.Web.Tests;

public sealed class HomeTests : BunitContext
{
    private readonly ControllablePokemonRepository _repository = new();

    public HomeTests()
    {
        Services.AddSingleton<IPokemonRepository>(_repository);
        Services.AddSingleton<IRandomIdProvider, FixedRandomIdProvider>();
        Services.AddSingleton(Options.Create(new PokedexOptions()));
        Services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        Services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        Services.AddTransient<GetRandomPokemonHandler>();
    }

    [Fact]
    public void InitiallyShowsAFriendlyHint()
    {
        var page = Render<Home>();

        Assert.Contains("Press the button", page.Find(".status.hint").TextContent);
        Assert.Empty(page.FindAll(".pokemon-card"));
    }

    [Fact]
    public void ShowsLoadingState_WhileFetching()
    {
        var page = Render<Home>();

        page.Find(".pick-button").Click();

        Assert.NotNull(page.Find(".status.loading"));
        Assert.True(page.Find(".pick-button").HasAttribute("disabled"));
    }

    [Fact]
    public void ShowsPokemon_WhenFetchSucceeds()
    {
        var page = Render<Home>();
        page.Find(".pick-button").Click();

        _repository.Succeed(ControllablePokemonRepository.Pikachu());

        page.WaitForAssertion(() => Assert.Equal("Pikachu", page.Find(".pokemon-card .name").TextContent));
        Assert.Empty(page.FindAll(".status.loading"));
        Assert.False(page.Find(".pick-button").HasAttribute("disabled"));
    }

    [Fact]
    public void ShowsFriendlyError_WhenFetchFails()
    {
        var page = Render<Home>();
        page.Find(".pick-button").Click();

        _repository.Fail();

        page.WaitForAssertion(() => Assert.Contains("try again", page.Find(".status.error").TextContent));
        Assert.NotNull(page.Find(".retry-button"));
    }

    [Fact]
    public void RetryButton_FetchesAgain()
    {
        var page = Render<Home>();
        page.Find(".pick-button").Click();
        _repository.Fail();
        page.WaitForElement(".retry-button").Click();

        _repository.Succeed(ControllablePokemonRepository.Pikachu());

        page.WaitForElement(".pokemon-card");
        Assert.Empty(page.FindAll(".status.error"));
    }

    [Fact]
    public async Task Dispose_CancelsInFlightRequest()
    {
        var page = Render<Home>();
        page.Find(".pick-button").Click();

        await DisposeComponentsAsync();

        Assert.True(_repository.LastToken.IsCancellationRequested);
    }
}

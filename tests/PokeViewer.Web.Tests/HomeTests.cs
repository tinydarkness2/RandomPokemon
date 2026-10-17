using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Application.Pokedex;
using PokeViewer.Application.Units;
using PokeViewer.Web.Units;
using PokeViewer.Web.Components.Pages;

namespace PokeViewer.Web.Tests;

public sealed class HomeTests : BunitContext
{
    private readonly ControllablePokemonRepository _repository = new();
    private readonly FakeUnitPreferenceStore _units = new(UnitSystem.Imperial);

    public HomeTests()
    {
        Services.AddSingleton<IPokemonRepository>(_repository);
        Services.AddSingleton<IRandomIdProvider, FixedRandomIdProvider>();
        Services.AddSingleton(Options.Create(new PokedexOptions()));
        Services.AddMoveTypeLookups(new FakeMoveTypeRepository());
        Services.AddSingleton<IUnitPreferenceStore>(_units);
        Services.AddTransient<GetRandomPokemonHandler>();
    }

    [Fact]
    public void ShowsTitleAndSubtitle()
    {
        var page = Render<Home>();

        Assert.Equal("Random Pokemon 🌈", page.Find("h1").TextContent);
        Assert.Equal("See a random Pokemon from generations 1 - 4 ✨", page.Find(".subtitle").TextContent);
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
        Assert.Equal("Thunder Shock", page.Find(".pokemon-card .move-name").TextContent);
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

    [Fact]
    public void ShowsUnitToggle_WithTheRememberedChoice()
    {
        var page = Render<Home>();

        page.WaitForAssertion(() => Assert.Equal("true", page.FindAll(".unit-toggle button")[1].GetAttribute("aria-pressed")));
    }

    [Fact]
    public void SwitchingUnits_UpdatesTheCardAndRemembersTheChoice()
    {
        var page = Render<Home>();
        page.Find(".pick-button").Click();
        _repository.Succeed(ControllablePokemonRepository.Pikachu());
        page.WaitForAssertion(() => Assert.Equal("1′04″", page.Find(".height [aria-hidden]").TextContent));

        page.FindAll(".unit-toggle button")[0].Click();

        Assert.Equal("0.4 m", page.Find(".height [aria-hidden]").TextContent);
        Assert.Equal("6.0 kg", page.Find(".weight [aria-hidden]").TextContent);
        Assert.Equal([UnitSystem.Metric], _units.Saved);
    }
}

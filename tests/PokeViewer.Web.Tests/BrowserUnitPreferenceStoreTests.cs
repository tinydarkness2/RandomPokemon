using Bunit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using PokeViewer.Application.Units;
using PokeViewer.Web.Units;

namespace PokeViewer.Web.Tests;

public sealed class BrowserUnitPreferenceStoreTests : BunitContext
{
    private const string ModulePath = "./js/unitPreference.js";

    private BrowserUnitPreferenceStore CreateStore() =>
        new(JSInterop.JSRuntime, NullLogger<BrowserUnitPreferenceStore>.Instance);

    private void SetupLoad(string? saved, string? language) =>
        JSInterop.SetupModule(ModulePath)
            .Setup<BrowserUnitPreferenceStore.StoredPreference>("load", "pokeviewer.units")
            .SetResult(new BrowserUnitPreferenceStore.StoredPreference(saved, language));

    [Fact]
    public async Task LoadAsync_ReturnsSavedChoice()
    {
        SetupLoad("Imperial", "de-DE");

        Assert.Equal(UnitSystem.Imperial, await CreateStore().LoadAsync());
    }

    [Theory]
    [InlineData("en-US", UnitSystem.Imperial)]
    [InlineData("en-GB", UnitSystem.Metric)]
    public async Task LoadAsync_DefaultsFromBrowserLanguage(string language, UnitSystem expected)
    {
        SetupLoad(null, language);

        Assert.Equal(expected, await CreateStore().LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_FallsBackToMetric_WhenInteropFails()
    {
        JSInterop.SetupModule(ModulePath)
            .Setup<BrowserUnitPreferenceStore.StoredPreference>("load", "pokeviewer.units")
            .SetException(new JSException("storage blocked"));

        Assert.Equal(UnitSystem.Metric, await CreateStore().LoadAsync());
    }

    [Fact]
    public async Task SaveAsync_StoresTheChoiceByName()
    {
        var module = JSInterop.SetupModule(ModulePath);
        module.SetupVoid("save", "pokeviewer.units", "Imperial").SetVoidResult();

        await CreateStore().SaveAsync(UnitSystem.Imperial);

        module.VerifyInvoke("save");
    }
}

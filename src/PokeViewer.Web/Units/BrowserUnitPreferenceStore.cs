using Microsoft.JSInterop;
using PokeViewer.Application.Units;

namespace PokeViewer.Web.Units;

/// <summary>Keeps the unit choice in the browser's local storage via a small JS module.</summary>
internal sealed partial class BrowserUnitPreferenceStore(IJSRuntime js, ILogger<BrowserUnitPreferenceStore> logger)
    : IUnitPreferenceStore, IAsyncDisposable
{
    private const string StorageKey = "pokeviewer.units";

    private IJSObjectReference? _module;

    public async Task<UnitSystem> LoadAsync()
    {
        try
        {
            var module = await GetModuleAsync();
            var preference = await module.InvokeAsync<StoredPreference>("load", StorageKey);
            return UnitSystemDefaults.Resolve(preference.Saved, preference.Language);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or TaskCanceledException)
        {
            LogInteropFailed(logger, ex);
            return UnitSystem.Metric;
        }
    }

    public async Task SaveAsync(UnitSystem units)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("save", StorageKey, units.ToString());
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or TaskCanceledException)
        {
            LogInteropFailed(logger, ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The circuit is already gone, and the module with it.
            }
        }
    }

    private async Task<IJSObjectReference> GetModuleAsync() =>
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/unitPreference.js");

    internal sealed record StoredPreference(string? Saved, string? Language);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Unit preference storage unavailable")]
    private static partial void LogInteropFailed(ILogger logger, Exception exception);
}

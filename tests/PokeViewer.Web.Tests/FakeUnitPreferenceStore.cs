using PokeViewer.Application.Units;
using PokeViewer.Web.Units;

namespace PokeViewer.Web.Tests;

internal sealed class FakeUnitPreferenceStore(UnitSystem initial = UnitSystem.Metric) : IUnitPreferenceStore
{
    public List<UnitSystem> Saved { get; } = [];

    public Task<UnitSystem> LoadAsync() => Task.FromResult(initial);

    public Task SaveAsync(UnitSystem units)
    {
        Saved.Add(units);
        return Task.CompletedTask;
    }
}

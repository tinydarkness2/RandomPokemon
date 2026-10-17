using PokeViewer.Application.Units;

namespace PokeViewer.Web.Units;

/// <summary>Remembers the visitor's unit system between visits.</summary>
public interface IUnitPreferenceStore
{
    /// <summary>The saved choice, or a default based on the browser's language.</summary>
    Task<UnitSystem> LoadAsync();

    Task SaveAsync(UnitSystem units);
}

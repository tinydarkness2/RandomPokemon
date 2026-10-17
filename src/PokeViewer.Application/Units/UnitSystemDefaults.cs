namespace PokeViewer.Application.Units;

public static class UnitSystemDefaults
{
    /// <summary>
    /// Picks the unit system for a visitor: their saved choice if it's valid, otherwise
    /// imperial for US English browsers and metric for everyone else.
    /// </summary>
    /// <param name="savedChoice">What was previously saved, e.g. "Imperial"; may be missing or stale.</param>
    /// <param name="browserLanguage">The browser's language tag, e.g. "en-US".</param>
    public static UnitSystem Resolve(string? savedChoice, string? browserLanguage)
    {
        if (Enum.TryParse<UnitSystem>(savedChoice, ignoreCase: true, out var saved) && Enum.IsDefined(saved))
        {
            return saved;
        }

        return string.Equals(browserLanguage, "en-US", StringComparison.OrdinalIgnoreCase)
            ? UnitSystem.Imperial
            : UnitSystem.Metric;
    }
}

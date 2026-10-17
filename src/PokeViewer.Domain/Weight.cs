namespace PokeViewer.Domain;

/// <summary>Weight as reported by PokeAPI, in hectograms.</summary>
public readonly record struct Weight
{
    public Weight(int hectograms)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(hectograms);
        Hectograms = hectograms;
    }

    public int Hectograms { get; }

    public decimal Kilograms => Hectograms / 10m;

    /// <summary>Weight in pounds to one decimal place, as the Pokédex games show it (69 hg → 15.2 lb).</summary>
    public decimal Pounds => Math.Round(Hectograms * PoundsPerHectogram, 1, MidpointRounding.AwayFromZero);

    private const decimal PoundsPerHectogram = 0.2204622622m;
}

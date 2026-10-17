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
}

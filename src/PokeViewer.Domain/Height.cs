namespace PokeViewer.Domain;

/// <summary>Height as reported by PokeAPI, in decimetres.</summary>
public readonly record struct Height
{
    public Height(int decimetres)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimetres);
        Decimetres = decimetres;
    }

    public int Decimetres { get; }

    public decimal Metres => Decimetres / 10m;

    /// <summary>Height in whole inches, rounded the way the Pokédex games do (7 dm → 28 in → 2′04″).</summary>
    public int TotalInches => (int)Math.Round(Decimetres * InchesPerDecimetre, MidpointRounding.AwayFromZero);

    private const decimal InchesPerDecimetre = 3.937007874m;
}

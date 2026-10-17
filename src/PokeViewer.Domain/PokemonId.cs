namespace PokeViewer.Domain;

/// <summary>National Pokédex number. Always positive.</summary>
public readonly record struct PokemonId
{
    public PokemonId(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        Value = value;
    }

    public int Value { get; }

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

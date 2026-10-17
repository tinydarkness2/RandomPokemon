namespace PokeViewer.Domain;

/// <summary>An elemental type such as "grass" or "poison", stored lower-case.</summary>
public sealed record PokemonType
{
    public PokemonType(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim().ToLowerInvariant();
    }

    public string Name { get; }

    public override string ToString() => Name;
}

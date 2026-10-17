namespace PokeViewer.Domain;

public sealed class Pokemon
{
    public Pokemon(
        PokemonId id,
        string name,
        IEnumerable<PokemonType> types,
        Height height,
        Weight weight,
        Uri? spriteUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(types);

        var typeList = types.ToList().AsReadOnly();
        if (typeList.Count == 0)
        {
            throw new ArgumentException("A Pokémon must have at least one type.", nameof(types));
        }

        Id = id;
        Name = name.Trim();
        Types = typeList;
        Height = height;
        Weight = weight;
        SpriteUrl = spriteUrl;
    }

    public PokemonId Id { get; }

    public string Name { get; }

    public IReadOnlyList<PokemonType> Types { get; }

    public Height Height { get; }

    public Weight Weight { get; }

    public Uri? SpriteUrl { get; }
}

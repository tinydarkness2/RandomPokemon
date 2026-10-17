namespace PokeViewer.Domain;

/// <summary>A move a Pokémon can learn, and how. <see cref="Level"/> is only meaningful for level-up moves.</summary>
public sealed record PokemonMove
{
    public PokemonMove(string name, MoveLearnMethod method, int level = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(level);

        Name = name.Trim().ToLowerInvariant();
        Method = method;
        Level = level;
    }

    public string Name { get; }

    public MoveLearnMethod Method { get; }

    public int Level { get; }
}

namespace PokeViewer.Domain;

/// <summary>The moves a Pokémon can learn in one game (PokeAPI "version group").</summary>
public sealed class Moveset
{
    public Moveset(string versionGroup, IEnumerable<PokemonMove> moves)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionGroup);
        ArgumentNullException.ThrowIfNull(moves);

        VersionGroup = versionGroup.Trim().ToLowerInvariant();
        Moves = moves.Distinct().ToList().AsReadOnly();
    }

    public string VersionGroup { get; }

    public IReadOnlyList<PokemonMove> Moves { get; }
}

using PokeViewer.Domain;

namespace PokeViewer.Application.Abstractions;

public interface IMoveTypeRepository
{
    /// <summary>Gets a move's type as it was in the given game (PokeAPI version group).</summary>
    /// <exception cref="PokemonUnavailableException">The move's type could not be retrieved.</exception>
    Task<PokemonType> GetMoveTypeAsync(string moveName, string versionGroup, CancellationToken cancellationToken);
}

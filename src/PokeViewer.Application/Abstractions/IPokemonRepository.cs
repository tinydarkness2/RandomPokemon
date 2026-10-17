using PokeViewer.Domain;

namespace PokeViewer.Application.Abstractions;

public interface IPokemonRepository
{
    /// <summary>Gets a Pokémon by its Pokédex number.</summary>
    /// <exception cref="PokemonUnavailableException">The Pokémon could not be retrieved.</exception>
    Task<Pokemon> GetByIdAsync(PokemonId id, CancellationToken cancellationToken);
}

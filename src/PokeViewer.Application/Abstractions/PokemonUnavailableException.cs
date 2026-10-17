namespace PokeViewer.Application.Abstractions;

/// <summary>Thrown by an <see cref="IPokemonRepository"/> when a Pokémon cannot be retrieved.</summary>
public sealed class PokemonUnavailableException : Exception
{
    public PokemonUnavailableException()
    {
    }

    public PokemonUnavailableException(string message)
        : base(message)
    {
    }

    public PokemonUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

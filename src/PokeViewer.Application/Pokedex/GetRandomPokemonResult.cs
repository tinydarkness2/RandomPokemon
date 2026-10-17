using System.Diagnostics.CodeAnalysis;

namespace PokeViewer.Application.Pokedex;

public sealed record GetRandomPokemonResult
{
    private GetRandomPokemonResult(PokemonDto? pokemon, string? errorMessage)
    {
        Pokemon = pokemon;
        ErrorMessage = errorMessage;
    }

    public PokemonDto? Pokemon { get; }

    public string? ErrorMessage { get; }

    [MemberNotNullWhen(true, nameof(Pokemon))]
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess => Pokemon is not null;

    public static GetRandomPokemonResult Success(PokemonDto pokemon) => new(pokemon, null);

    public static GetRandomPokemonResult Failure(string errorMessage) => new(null, errorMessage);
}

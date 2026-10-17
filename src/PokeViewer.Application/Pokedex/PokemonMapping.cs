using System.Globalization;
using PokeViewer.Domain;

namespace PokeViewer.Application.Pokedex;

internal static class PokemonMapping
{
    public static PokemonDto ToDto(this Pokemon pokemon) => new(
        pokemon.Id.Value,
        ToDisplayName(pokemon.Name),
        pokemon.Types.Select(t => ToDisplayName(t.Name)).ToList().AsReadOnly(),
        pokemon.Height.Metres,
        pokemon.Weight.Kilograms,
        pokemon.SpriteUrl?.AbsoluteUri);

    /// <summary>Turns API names like "mr-mime" into "Mr Mime".</summary>
    internal static string ToDisplayName(string name) =>
        string.Join(' ', name
            .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(part.ToLowerInvariant())));
}

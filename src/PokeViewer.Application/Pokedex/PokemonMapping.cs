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
        pokemon.Height.TotalInches,
        pokemon.Weight.Kilograms,
        pokemon.Weight.Pounds,
        pokemon.SpriteUrl?.AbsoluteUri,
        pokemon.Moveset.ToDto());

    /// <summary>Groups moves by how they are learned: level-up moves by level, the rest alphabetically.</summary>
    internal static MovesetDto ToDto(this Moveset moveset) => new(
        moveset.VersionGroup,
        ToDisplayName(moveset.VersionGroup),
        moveset.Moves.Select(m => m.Name).Distinct().Count(),
        moveset.Moves
            .GroupBy(m => m.Method)
            .OrderBy(g => g.Key)
            .Select(g => new MoveGroupDto(
                GroupTitle(g.Key),
                g.OrderBy(m => g.Key == MoveLearnMethod.LevelUp ? m.Level : 0)
                    .ThenBy(m => m.Name, StringComparer.Ordinal)
                    .Select(m => new MoveDto(
                        m.Name,
                        ToDisplayName(m.Name),
                        g.Key == MoveLearnMethod.LevelUp ? m.Level : null))
                    .ToList()
                    .AsReadOnly()))
            .ToList()
            .AsReadOnly());

    /// <summary>Gen 2–4's "???" type (Curse) is called "unknown" by PokeAPI; show it the way the games did.</summary>
    internal static MoveTypeDto ToMoveTypeDto(this PokemonType type) =>
        new(type.Name, type.Name == "unknown" ? "???" : ToDisplayName(type.Name));

    internal static string GroupTitle(MoveLearnMethod method) => method switch
    {
        MoveLearnMethod.LevelUp => "Level up",
        MoveLearnMethod.Machine => "TM / HM",
        MoveLearnMethod.Egg => "Egg moves",
        MoveLearnMethod.Tutor => "Move tutor",
        _ => "Other",
    };

    /// <summary>Turns API names like "mr-mime" into "Mr Mime".</summary>
    internal static string ToDisplayName(string name) =>
        string.Join(' ', name
            .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(part.ToLowerInvariant())));
}

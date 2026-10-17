namespace PokeViewer.Application.Pokedex;

public sealed record PokemonDto(
    int Id,
    string Name,
    IReadOnlyList<string> Types,
    decimal HeightMetres,
    decimal WeightKilograms,
    string? SpriteUrl,
    MovesetDto Moveset);

/// <param name="VersionGroup">The PokeAPI version group the moves come from, e.g. "heartgold-soulsilver".</param>
/// <param name="GameName">That game's display name, e.g. "Heartgold Soulsilver".</param>
/// <param name="MoveCount">Distinct moves across all groups (a move can appear in more than one group).</param>
public sealed record MovesetDto(string VersionGroup, string GameName, int MoveCount, IReadOnlyList<MoveGroupDto> Groups);

public sealed record MoveGroupDto(string Title, IReadOnlyList<MoveDto> Moves);

/// <param name="Key">The PokeAPI move name, e.g. "vine-whip".</param>
/// <param name="Name">The display name, e.g. "Vine Whip".</param>
/// <param name="Level">Set only for level-up moves.</param>
public sealed record MoveDto(string Key, string Name, int? Level);

/// <param name="Key">The PokeAPI type name, e.g. "grass" or "unknown"; handy for styling.</param>
/// <param name="Name">The display name, e.g. "Grass" or "???".</param>
public sealed record MoveTypeDto(string Key, string Name);

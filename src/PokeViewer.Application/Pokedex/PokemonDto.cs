namespace PokeViewer.Application.Pokedex;

public sealed record PokemonDto(
    int Id,
    string Name,
    IReadOnlyList<string> Types,
    decimal HeightMetres,
    decimal WeightKilograms,
    string? SpriteUrl,
    MovesetDto Moveset);

/// <param name="GameName">The game the moves come from, e.g. "Heartgold Soulsilver".</param>
/// <param name="MoveCount">Distinct moves across all groups (a move can appear in more than one group).</param>
public sealed record MovesetDto(string GameName, int MoveCount, IReadOnlyList<MoveGroupDto> Groups);

public sealed record MoveGroupDto(string Title, IReadOnlyList<MoveDto> Moves);

/// <param name="Level">Set only for level-up moves.</param>
public sealed record MoveDto(string Name, int? Level);

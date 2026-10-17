namespace PokeViewer.Application.Pokedex;

public sealed record PokemonDto(
    int Id,
    string Name,
    IReadOnlyList<string> Types,
    decimal HeightMetres,
    decimal WeightKilograms,
    string? SpriteUrl);

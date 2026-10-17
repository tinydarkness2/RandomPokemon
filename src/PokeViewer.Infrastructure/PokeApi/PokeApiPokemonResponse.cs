using System.Text.Json.Serialization;

namespace PokeViewer.Infrastructure.PokeApi;

// Only the fields we use from GET /pokemon/{id}.
internal sealed record PokeApiPokemonResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("weight")] int Weight,
    [property: JsonPropertyName("types")] IReadOnlyList<PokeApiTypeSlot> Types,
    [property: JsonPropertyName("sprites")] PokeApiSprites? Sprites);

internal sealed record PokeApiTypeSlot(
    [property: JsonPropertyName("slot")] int Slot,
    [property: JsonPropertyName("type")] PokeApiNamedResource Type);

internal sealed record PokeApiNamedResource(
    [property: JsonPropertyName("name")] string Name);

internal sealed record PokeApiSprites(
    [property: JsonPropertyName("front_default")] string? FrontDefault,
    [property: JsonPropertyName("other")] PokeApiOtherSprites? Other);

internal sealed record PokeApiOtherSprites(
    [property: JsonPropertyName("official-artwork")] PokeApiArtwork? OfficialArtwork);

internal sealed record PokeApiArtwork(
    [property: JsonPropertyName("front_default")] string? FrontDefault);

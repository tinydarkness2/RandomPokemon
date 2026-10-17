using System.Text.Json.Serialization;

namespace PokeViewer.Infrastructure.PokeApi;

// Only the fields we use from GET /pokemon/{id}.
internal sealed record PokeApiPokemonResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("weight")] int Weight,
    [property: JsonPropertyName("types")] IReadOnlyList<PokeApiTypeSlot> Types,
    [property: JsonPropertyName("sprites")] PokeApiSprites? Sprites,
    [property: JsonPropertyName("moves")] IReadOnlyList<PokeApiMoveSlot>? Moves);

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

internal sealed record PokeApiMoveSlot(
    [property: JsonPropertyName("move")] PokeApiNamedResource Move,
    [property: JsonPropertyName("version_group_details")] IReadOnlyList<PokeApiMoveVersionDetail> VersionGroupDetails);

internal sealed record PokeApiMoveVersionDetail(
    [property: JsonPropertyName("level_learned_at")] int LevelLearnedAt,
    [property: JsonPropertyName("move_learn_method")] PokeApiNamedResource MoveLearnMethod,
    [property: JsonPropertyName("version_group")] PokeApiNamedResource VersionGroup);

// Only the fields we use from GET /move/{name}.
internal sealed record PokeApiMoveResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] PokeApiNamedResource Type,
    [property: JsonPropertyName("past_values")] IReadOnlyList<PokeApiMovePastValue>? PastValues);

/// <summary>Values the move had before <see cref="VersionGroup"/>; <see cref="Type"/> is null when the type didn't change then.</summary>
internal sealed record PokeApiMovePastValue(
    [property: JsonPropertyName("type")] PokeApiNamedResource? Type,
    [property: JsonPropertyName("version_group")] PokeApiNamedResource VersionGroup);

// Only the fields we use from GET /version-group/{name}.
internal sealed record PokeApiVersionGroupResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("order")] int Order);

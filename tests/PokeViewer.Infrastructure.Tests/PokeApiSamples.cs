namespace PokeViewer.Infrastructure.Tests;

internal static class PokeApiSamples
{
    // A trimmed-down copy of GET /pokemon/1, including fields we ignore.
    public const string Bulbasaur = """
        {
          "id": 1,
          "name": "bulbasaur",
          "height": 7,
          "weight": 69,
          "base_experience": 64,
          "types": [
            { "slot": 2, "type": { "name": "poison", "url": "https://pokeapi.co/api/v2/type/4/" } },
            { "slot": 1, "type": { "name": "grass", "url": "https://pokeapi.co/api/v2/type/12/" } }
          ],
          "sprites": {
            "front_default": "https://img.example.test/sprites/1.png",
            "other": {
              "official-artwork": { "front_default": "https://img.example.test/artwork/1.png" }
            }
          }
        }
        """;

    public const string NoArtwork = """
        {
          "id": 132,
          "name": "ditto",
          "height": 3,
          "weight": 40,
          "types": [ { "slot": 1, "type": { "name": "normal" } } ],
          "sprites": { "front_default": "https://img.example.test/sprites/132.png", "other": {} }
        }
        """;

    public const string NoSprites = """
        { "id": 201, "name": "unown", "height": 5, "weight": 50,
          "types": [ { "slot": 1, "type": { "name": "psychic" } } ], "sprites": null }
        """;
}

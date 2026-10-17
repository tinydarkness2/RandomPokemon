using System.ComponentModel.DataAnnotations;

namespace PokeViewer.Infrastructure.PokeApi;

public sealed class PokeApiOptions
{
    public const string SectionName = "PokeApi";

    [Required]
    public Uri BaseUrl { get; set; } = new("https://pokeapi.co/api/v2/");

    /// <summary>How long a fetched Pokémon stays in the memory cache.</summary>
    [Range(typeof(TimeSpan), "00:00:01", "7.00:00:00")]
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>The PokeAPI version group (game) to take movesets from.</summary>
    [Required]
    [RegularExpression("^[a-z0-9-]+$")]
    public string MoveVersionGroup { get; set; } = "heartgold-soulsilver";
}

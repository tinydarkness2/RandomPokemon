using System.ComponentModel.DataAnnotations;

namespace PokeViewer.Application.Pokedex;

/// <summary>The range of Pokédex numbers to pick random Pokémon from.</summary>
public sealed class PokedexOptions : IValidatableObject
{
    public const string SectionName = "PokeApi";

    [Range(1, int.MaxValue)]
    public int MinId { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int MaxId { get; set; } = 493;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinId > MaxId)
        {
            yield return new ValidationResult(
                $"{nameof(MinId)} ({MinId}) must not be greater than {nameof(MaxId)} ({MaxId}).",
                [nameof(MinId), nameof(MaxId)]);
        }
    }
}

using System.Globalization;
using PokeViewer.Application.Pokedex;

namespace PokeViewer.Application.Units;

/// <param name="Text">What to show, e.g. "2′04″".</param>
/// <param name="SpokenText">What a screen reader should say, e.g. "2 feet 4 inches".</param>
public sealed record FormattedMeasurement(string Text, string SpokenText);

public static class Measurements
{
    public static FormattedMeasurement FormatHeight(PokemonDto pokemon, UnitSystem units)
    {
        ArgumentNullException.ThrowIfNull(pokemon);

        if (units == UnitSystem.Imperial)
        {
            var feet = pokemon.HeightInches / 12;
            var inches = pokemon.HeightInches % 12;
            return new(
                string.Create(CultureInfo.InvariantCulture, $"{feet}′{inches:00}″"),
                $"{Count(feet, "foot", "feet")} {Count(inches, "inch", "inches")}");
        }

        var metres = pokemon.HeightMetres.ToString("0.0", CultureInfo.InvariantCulture);
        return new($"{metres} m", $"{metres} metres");
    }

    public static FormattedMeasurement FormatWeight(PokemonDto pokemon, UnitSystem units)
    {
        ArgumentNullException.ThrowIfNull(pokemon);

        if (units == UnitSystem.Imperial)
        {
            var pounds = pokemon.WeightPounds.ToString("0.0", CultureInfo.InvariantCulture);
            return new($"{pounds} lb", $"{pounds} pounds");
        }

        var kilograms = pokemon.WeightKilograms.ToString("0.0", CultureInfo.InvariantCulture);
        return new($"{kilograms} kg", $"{kilograms} kilograms");
    }

    private static string Count(int value, string singular, string plural) =>
        string.Create(CultureInfo.InvariantCulture, $"{value} {(value == 1 ? singular : plural)}");
}

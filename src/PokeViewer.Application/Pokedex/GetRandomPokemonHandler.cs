using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Domain;

namespace PokeViewer.Application.Pokedex;

public sealed partial class GetRandomPokemonHandler(
    IPokemonRepository repository,
    IRandomIdProvider randomIdProvider,
    IOptions<PokedexOptions> options,
    ILogger<GetRandomPokemonHandler> logger)
{
    internal const string FriendlyErrorMessage =
        "We couldn't reach the Pokédex just now. Please try again in a moment.";

    public async Task<GetRandomPokemonResult> HandleAsync(CancellationToken cancellationToken)
    {
        var range = options.Value;
        var id = new PokemonId(randomIdProvider.NextInRange(range.MinId, range.MaxId));

        try
        {
            var pokemon = await repository.GetByIdAsync(id, cancellationToken);
            return GetRandomPokemonResult.Success(pokemon.ToDto());
        }
        catch (PokemonUnavailableException ex)
        {
            LogPokemonUnavailable(logger, id.Value, ex);
            return GetRandomPokemonResult.Failure(FriendlyErrorMessage);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Pokémon #{PokemonId} could not be retrieved")]
    private static partial void LogPokemonUnavailable(ILogger logger, int pokemonId, Exception exception);
}

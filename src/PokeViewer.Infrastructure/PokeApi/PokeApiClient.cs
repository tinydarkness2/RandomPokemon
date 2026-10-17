using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Domain;
using Polly;

namespace PokeViewer.Infrastructure.PokeApi;

/// <summary>Typed HttpClient for PokeAPI. Its BaseAddress is set at registration.</summary>
internal sealed partial class PokeApiClient(
    HttpClient httpClient,
    IOptions<PokeApiOptions> options,
    ILogger<PokeApiClient> logger)
{
    public async Task<Pokemon> GetPokemonAsync(PokemonId id, CancellationToken cancellationToken)
    {
        LogFetching(logger, id.Value);

        try
        {
            using var response = await httpClient.GetAsync($"pokemon/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new PokemonUnavailableException($"Pokémon #{id} was not found.");
            }

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<PokeApiPokemonResponse>(cancellationToken)
                ?? throw new PokemonUnavailableException($"PokeAPI returned an empty body for Pokémon #{id}.");

            return ToDomain(body, options.Value.MoveVersionGroup);
        }
        catch (Exception ex) when (IsTransportFailure(ex, cancellationToken))
        {
            throw new PokemonUnavailableException($"PokeAPI request for Pokémon #{id} failed.", ex);
        }
    }

    private static bool IsTransportFailure(Exception ex, CancellationToken cancellationToken) => ex switch
    {
        HttpRequestException or JsonException or ArgumentException or ExecutionRejectedException => true,
        // A timeout surfaces as a cancellation that the caller did not ask for.
        OperationCanceledException => !cancellationToken.IsCancellationRequested,
        _ => false,
    };

    private static Pokemon ToDomain(PokeApiPokemonResponse body, string moveVersionGroup)
    {
        var spriteUrl = body.Sprites?.Other?.OfficialArtwork?.FrontDefault ?? body.Sprites?.FrontDefault;

        return new Pokemon(
            new PokemonId(body.Id),
            body.Name,
            body.Types.OrderBy(t => t.Slot).Select(t => new PokemonType(t.Type.Name)),
            new Height(body.Height),
            new Weight(body.Weight),
            Uri.TryCreate(spriteUrl, UriKind.Absolute, out var uri) ? uri : null,
            ToMoveset(body.Moves ?? [], moveVersionGroup));
    }

    private static Moveset ToMoveset(IEnumerable<PokeApiMoveSlot> slots, string versionGroup) => new(
        versionGroup,
        from slot in slots
        from detail in slot.VersionGroupDetails
        where detail.VersionGroup.Name == versionGroup
        let method = ToLearnMethod(detail.MoveLearnMethod.Name)
        select new PokemonMove(
            slot.Move.Name,
            method,
            method == MoveLearnMethod.LevelUp ? detail.LevelLearnedAt : 0));

    private static MoveLearnMethod ToLearnMethod(string apiName) => apiName switch
    {
        "level-up" => MoveLearnMethod.LevelUp,
        "machine" => MoveLearnMethod.Machine,
        "egg" => MoveLearnMethod.Egg,
        "tutor" => MoveLearnMethod.Tutor,
        _ => MoveLearnMethod.Other,
    };

    [LoggerMessage(Level = LogLevel.Debug, Message = "Fetching Pokémon #{PokemonId} from PokeAPI")]
    private static partial void LogFetching(ILogger logger, int pokemonId);
}

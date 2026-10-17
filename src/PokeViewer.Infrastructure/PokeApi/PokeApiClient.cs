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
    public Task<Pokemon> GetPokemonAsync(PokemonId id, CancellationToken cancellationToken) =>
        GetAsync<PokeApiPokemonResponse, Pokemon>(
            $"pokemon/{id}",
            $"Pokémon #{id}",
            body => ToDomain(body, options.Value.MoveVersionGroup),
            cancellationToken);

    public Task<PokeApiMoveResponse> GetMoveAsync(string moveName, CancellationToken cancellationToken) =>
        GetAsync<PokeApiMoveResponse, PokeApiMoveResponse>(
            $"move/{Uri.EscapeDataString(moveName)}",
            $"Move '{moveName}'",
            body => body,
            cancellationToken);

    public Task<int> GetVersionGroupOrderAsync(string versionGroup, CancellationToken cancellationToken) =>
        GetAsync<PokeApiVersionGroupResponse, int>(
            $"version-group/{Uri.EscapeDataString(versionGroup)}",
            $"Version group '{versionGroup}'",
            body => body.Order,
            cancellationToken);

    /// <summary>
    /// GETs <paramref name="path"/> and maps the JSON body. Every failure, including an
    /// unusable body, becomes a <see cref="PokemonUnavailableException"/>.
    /// </summary>
    private async Task<TResult> GetAsync<TBody, TResult>(
        string path,
        string description,
        Func<TBody, TResult> map,
        CancellationToken cancellationToken)
    {
        LogFetching(logger, path);

        try
        {
            using var response = await httpClient.GetAsync(path, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new PokemonUnavailableException($"{description} was not found.");
            }

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<TBody>(cancellationToken)
                ?? throw new PokemonUnavailableException($"PokeAPI returned an empty body for {description}.");

            return map(body);
        }
        catch (Exception ex) when (IsTransportFailure(ex, cancellationToken))
        {
            throw new PokemonUnavailableException($"PokeAPI request for {description} failed.", ex);
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

    [LoggerMessage(Level = LogLevel.Debug, Message = "Fetching {Path} from PokeAPI")]
    private static partial void LogFetching(ILogger logger, string path);
}

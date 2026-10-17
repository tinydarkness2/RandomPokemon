using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PokeViewer.Application.Abstractions;

namespace PokeViewer.Application.Pokedex;

/// <summary>Looks up the type of each move in a moveset, a few at a time.</summary>
public sealed partial class GetMoveTypesHandler(
    IMoveTypeRepository repository,
    ILogger<GetMoveTypesHandler> logger)
{
    internal const int MaxConcurrentLookups = 6;

    /// <returns>
    /// Move types keyed by move key. Moves whose type could not be retrieved
    /// are left out, so the UI simply shows them uncoloured.
    /// </returns>
    public async Task<IReadOnlyDictionary<string, MoveTypeDto>> HandleAsync(
        MovesetDto moveset,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(moveset);

        var moveKeys = moveset.Groups.SelectMany(g => g.Moves).Select(m => m.Key).Distinct(StringComparer.Ordinal);
        var types = new ConcurrentDictionary<string, MoveTypeDto>(StringComparer.Ordinal);

        await Parallel.ForEachAsync(
            moveKeys,
            new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentLookups, CancellationToken = cancellationToken },
            async (moveKey, ct) =>
            {
                try
                {
                    var type = await repository.GetMoveTypeAsync(moveKey, moveset.VersionGroup, ct);
                    types[moveKey] = type.ToMoveTypeDto();
                }
                catch (PokemonUnavailableException ex)
                {
                    LogMoveTypeUnavailable(logger, moveKey, ex);
                }
            });

        return types;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Type of move {MoveName} could not be retrieved")]
    private static partial void LogMoveTypeUnavailable(ILogger logger, string moveName, Exception exception);
}

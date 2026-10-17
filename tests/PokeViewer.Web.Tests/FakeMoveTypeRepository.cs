using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PokeViewer.Application.Abstractions;
using PokeViewer.Application.Pokedex;
using PokeViewer.Domain;

namespace PokeViewer.Web.Tests;

/// <summary>Answers move-type lookups from a dictionary; unknown moves fail like an unreachable API.</summary>
internal sealed class FakeMoveTypeRepository : IMoveTypeRepository
{
    public Dictionary<string, string> Types { get; } = new(StringComparer.Ordinal);

    public List<(string Move, string VersionGroup)> Requests { get; } = [];

    /// <summary>When set, lookups wait for this task, so tests can observe the loading state.</summary>
    public Task? Gate { get; set; }

    public async Task<PokemonType> GetMoveTypeAsync(string moveName, string versionGroup, CancellationToken cancellationToken)
    {
        lock (Requests)
        {
            Requests.Add((moveName, versionGroup));
        }

        if (Gate is not null)
        {
            await Gate.WaitAsync(cancellationToken);
        }

        return Types.TryGetValue(moveName, out var type)
            ? new PokemonType(type)
            : throw new PokemonUnavailableException($"No type for {moveName}.");
    }
}

internal static class TestServiceExtensions
{
    public static IServiceCollection AddMoveTypeLookups(this IServiceCollection services, FakeMoveTypeRepository repository)
    {
        services.AddSingleton<IMoveTypeRepository>(repository);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddTransient<GetMoveTypesHandler>();
        return services;
    }
}

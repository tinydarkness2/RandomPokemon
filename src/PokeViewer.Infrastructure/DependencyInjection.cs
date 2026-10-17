using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Infrastructure.PokeApi;

namespace PokeViewer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PokeApiOptions>()
            .Bind(configuration.GetSection(PokeApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMemoryCache();

        services.AddHttpClient<PokeApiClient>((sp, client) =>
            {
                var baseUrl = sp.GetRequiredService<IOptions<PokeApiOptions>>().Value.BaseUrl.AbsoluteUri;
                // A trailing slash makes relative paths like "pokemon/1" append rather than replace.
                client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
            })
            // Blazor Server components can hold this typed client for a whole circuit, so let
            // the pooled connections recycle themselves to pick up DNS changes.
            .UseSocketsHttpHandler((handler, _) => handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2))
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddStandardResilienceHandler();

        services.AddScoped<IPokemonRepository, CachedPokemonRepository>();
        services.AddSingleton<IRandomIdProvider, SystemRandomIdProvider>();

        return services;
    }
}

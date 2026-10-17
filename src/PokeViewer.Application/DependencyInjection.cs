using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeViewer.Application.Pokedex;

namespace PokeViewer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PokedexOptions>()
            .Bind(configuration.GetSection(PokedexOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<GetRandomPokemonHandler>();

        return services;
    }
}

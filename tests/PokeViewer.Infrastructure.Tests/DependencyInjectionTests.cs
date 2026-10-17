using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Abstractions;
using PokeViewer.Infrastructure.PokeApi;

namespace PokeViewer.Infrastructure.Tests;

public class DependencyInjectionTests
{
    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection()
            .AddLogging()
            .AddInfrastructure(configuration)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    [Fact]
    public void ResolvesRepositoryAndRandomProvider()
    {
        using var provider = BuildProvider([]);
        using var scope = provider.CreateScope();

        Assert.IsType<CachedPokemonRepository>(scope.ServiceProvider.GetRequiredService<IPokemonRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IRandomIdProvider>());
    }

    [Theory]
    [InlineData("https://pokeapi.test/api/v2")]
    [InlineData("https://pokeapi.test/api/v2/")]
    public void TypedClient_BaseAddressAlwaysEndsWithSlash(string configured)
    {
        using var provider = BuildProvider(new() { ["PokeApi:BaseUrl"] = configured });

        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(PokeApiClient));

        Assert.Equal(new Uri("https://pokeapi.test/api/v2/"), httpClient.BaseAddress);
    }

    [Theory]
    [InlineData("PokeApi:CacheDuration", "00:00:00")]
    [InlineData("PokeApi:MoveVersionGroup", "")]
    [InlineData("PokeApi:MoveVersionGroup", "Heart Gold")]
    public void RejectsInvalidSettings(string key, string value)
    {
        using var provider = BuildProvider(new() { [key] = value });

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<PokeApiOptions>>().Value);
    }
}

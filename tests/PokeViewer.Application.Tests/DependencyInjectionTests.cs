using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PokeViewer.Application.Pokedex;

namespace PokeViewer.Application.Tests;

public class DependencyInjectionTests
{
    private static IOptions<PokedexOptions> ResolveOptions(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var provider = new ServiceCollection().AddApplication(configuration).BuildServiceProvider();
        return provider.GetRequiredService<IOptions<PokedexOptions>>();
    }

    [Fact]
    public void DefaultsToGenerationsOneToFour()
    {
        var options = ResolveOptions([]).Value;

        Assert.Equal(1, options.MinId);
        Assert.Equal(493, options.MaxId);
    }

    [Fact]
    public void BindsRangeFromConfiguration()
    {
        var options = ResolveOptions(new() { ["PokeApi:MinId"] = "1", ["PokeApi:MaxId"] = "151" }).Value;

        Assert.Equal(151, options.MaxId);
    }

    [Theory]
    [InlineData("0", "151")]
    [InlineData("200", "100")]
    public void RejectsInvalidRanges(string min, string max)
    {
        var options = ResolveOptions(new() { ["PokeApi:MinId"] = min, ["PokeApi:MaxId"] = max });

        Assert.Throws<OptionsValidationException>(() => options.Value);
    }
}

using PokeViewer.Application.Pokedex;

namespace PokeViewer.Application.Tests;

public class PokemonMappingTests
{
    [Theory]
    [InlineData("pikachu", "Pikachu")]
    [InlineData("mr-mime", "Mr Mime")]
    [InlineData("HO-OH", "Ho Oh")]
    [InlineData("porygon-z", "Porygon Z")]
    public void ToDisplayName_TitleCasesHyphenatedNames(string apiName, string expected) =>
        Assert.Equal(expected, PokemonMapping.ToDisplayName(apiName));
}

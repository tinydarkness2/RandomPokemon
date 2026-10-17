namespace PokeViewer.Infrastructure.Tests;

public class SystemRandomIdProviderTests
{
    [Fact]
    public void NextInRange_StaysWithinInclusiveBounds()
    {
        var provider = new SystemRandomIdProvider();

        var values = Enumerable.Range(0, 1_000).Select(_ => provider.NextInRange(1, 3)).ToHashSet();

        Assert.Equal([1, 2, 3], values.Order());
    }
}

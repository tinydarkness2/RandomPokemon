namespace PokeViewer.Application.Abstractions;

public interface IRandomIdProvider
{
    /// <summary>Returns a random integer between <paramref name="minInclusive"/> and <paramref name="maxInclusive"/>.</summary>
    int NextInRange(int minInclusive, int maxInclusive);
}

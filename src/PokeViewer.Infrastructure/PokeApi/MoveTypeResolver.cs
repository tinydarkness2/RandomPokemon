namespace PokeViewer.Infrastructure.PokeApi;

internal static class MoveTypeResolver
{
    /// <summary>
    /// Works out a move's type in a given game. PokeAPI reports the current type plus
    /// "past values": each entry holds what the move was like before that game's change.
    /// So the type in effect is the one recorded by the first type change made after the
    /// target game, or the current type if every change came before it.
    /// </summary>
    /// <param name="versionGroupOrders">PokeAPI "order" of the target and every changed version group.</param>
    public static string Resolve(PokeApiMoveResponse move, string targetVersionGroup, IReadOnlyDictionary<string, int> versionGroupOrders)
    {
        var typeChanges = (move.PastValues ?? []).Where(p => p.Type is not null).ToList();
        if (typeChanges.Count == 0)
        {
            return move.Type.Name;
        }

        var targetOrder = versionGroupOrders[targetVersionGroup];

        var typeAtTarget = typeChanges
            .Select(p => (Order: versionGroupOrders[p.VersionGroup.Name], Type: p.Type!.Name))
            .Where(p => p.Order > targetOrder)
            .OrderBy(p => p.Order)
            .Select(p => p.Type)
            .FirstOrDefault();

        return typeAtTarget ?? move.Type.Name;
    }

    /// <summary>The version groups whose order <see cref="Resolve"/> needs.</summary>
    public static IEnumerable<string> VersionGroupsNeeded(PokeApiMoveResponse move, string targetVersionGroup)
    {
        var changes = (move.PastValues ?? []).Where(p => p.Type is not null).Select(p => p.VersionGroup.Name).ToList();
        return changes.Count == 0 ? [] : changes.Append(targetVersionGroup).Distinct(StringComparer.Ordinal);
    }
}

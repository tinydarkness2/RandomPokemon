namespace PokeViewer.Domain;

/// <summary>How a Pokémon learns a move. Declared in the order the UI lists them.</summary>
public enum MoveLearnMethod
{
    LevelUp,
    Machine,
    Egg,
    Tutor,
    Other,
}

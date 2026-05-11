namespace FrontLine.Models
{
    public enum GridObjectType
    {
        Wall,             // TileType.Blocked
        HalfCover,        // TileType.Blocked (alpha)
        FullCover,        // TileType.Blocked (alpha)
        Decoration,       // visual only, tile unchanged
        FragGrenadeBox,   // collectable
        SmokeGrenadeBox   // collectable
    }
}

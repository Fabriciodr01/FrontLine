namespace FrontLine.Models
{
    public enum TileType
    {
        Normal,
        Blocked
    }

    public class TileData
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public TileType Type { get; set; }
        public bool IsOccupied { get; set; }
        public string OccupyingUnitId { get; set; }

        public TileData(int x, int y, TileType type = TileType.Normal)
        {
            X = x;
            Y = y;
            Type = type;
            IsOccupied = false;
            OccupyingUnitId = null;
        }

        public bool IsWalkable() => Type != TileType.Blocked && !IsOccupied;

        public override string ToString() => $"Tile({X},{Y}) Type:{Type} Occupied:{IsOccupied}";
    }
}

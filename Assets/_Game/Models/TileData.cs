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
        public int Elevation { get; set; } // 0 = ground, 1 = raised, 2 = rooftop etc.
        public TileType Type { get; set; }
        public bool IsOccupied { get; set; }
        public string OccupyingUnitId { get; set; }
        public bool HasSmoke { get; set; }
        public int SmokeTurnsRemaining { get; set; }

        public TileData(int x, int y, TileType type = TileType.Normal, int elevation = 0)
        {
            X = x;
            Y = y;
            Type = type;
            Elevation = elevation;
            IsOccupied = false;
            OccupyingUnitId = null;
        }

        public void ApplySmoke(int turns)
        {
            HasSmoke = true;
            SmokeTurnsRemaining = turns;
        }

        public void TickSmoke()
        {
            if (!HasSmoke) return;
            SmokeTurnsRemaining--;
            if (SmokeTurnsRemaining < 0) //smoke thicks at thrown turn
                HasSmoke = false;
        }

        public bool IsWalkable() => Type != TileType.Blocked && !IsOccupied;

        public override string ToString() => $"Tile({X},{Y}) Type:{Type} Occupied:{IsOccupied}";
    }
}

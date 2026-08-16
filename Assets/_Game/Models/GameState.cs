using System.Collections.Generic;
using FrontLine.Map.Grid;

namespace FrontLine.Models
{
    public enum GamePhase
    {
        WaitingForPlayers,
        Setup,
        Player1Turn,
        Player2Turn,
        GameOver
    }

    public class GameState
    {
        public GamePhase CurrentPhase { get; set; }
        public string WinnerPlayerId { get; set; }
        public int TurnNumber { get; set; }

        public Dictionary<string, UnitData> Units { get; private set; }
        public TileData[,] Grid { get; private set; }
        public Dictionary<(int, int), GrenadeBox> GrenadeBoxes { get; private set; }
        public List<PendingFragGrenade> PendingFragGrenades { get; private set; }

        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

        public GameState(int gridWidth, int gridHeight)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            CurrentPhase = GamePhase.WaitingForPlayers;
            TurnNumber = 0;
            WinnerPlayerId = null;
            Units = new Dictionary<string, UnitData>();
            Grid = new TileData[gridWidth, gridHeight];
            GrenadeBoxes = new Dictionary<(int, int), GrenadeBox>();
            PendingFragGrenades = new List<PendingFragGrenade>();

            InitializeGrid();
        }

        private void InitializeGrid()
        {
            for (int x = 0; x < GridWidth; x++)
                for (int y = 0; y < GridHeight; y++)
                    Grid[x, y] = new TileData(x, y);
        }

        public TileData GetTile(int x, int y)
        {
            if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
                return null;
            return Grid[x, y];
        }

        public bool IsValidPosition(int x, int y)
            => x >= 0 && x < GridWidth && y >= 0 && y < GridHeight;

        public bool IsUnitInSmoke(UnitData unit)
        {
            if (unit == null)
            {
                return false;
            }

            var tile = GetTile(unit.TileX, unit.TileY);
            return tile != null && tile.HasSmoke;
        }

        public void AddUnit(UnitData unit)
        {
            Units[unit.UnitId] = unit;
            var tile = GetTile(unit.TileX, unit.TileY);
            if (tile != null)
            {
                tile.IsOccupied = true;
                tile.OccupyingUnitId = unit.UnitId;
            }
        }

        public void RemoveUnit(string unitId)
        {
            if (!Units.ContainsKey(unitId)) return;
            var unit = Units[unitId];
            var tile = GetTile(unit.TileX, unit.TileY);
            if (tile != null)
            {
                tile.IsOccupied = false;
                tile.OccupyingUnitId = null;
            }
            Units.Remove(unitId);
        }

        public void AddGrenadeBox(GrenadeBox box)
            => GrenadeBoxes[(box.TileX, box.TileY)] = box;

        public void RemoveGrenadeBox(int x, int y)
            => GrenadeBoxes.Remove((x, y));
    }

    public class PendingFragGrenade
    {
        public int TargetX { get; private set; }
        public int TargetY { get; private set; }
        public int TurnsRemaining { get; set; }

        public PendingFragGrenade(int targetX, int targetY, int turnsRemaining)
        {
            TargetX = targetX;
            TargetY = targetY;
            TurnsRemaining = turnsRemaining;
        }
    }
}

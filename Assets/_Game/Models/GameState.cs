using System.Collections.Generic;

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
    }
}

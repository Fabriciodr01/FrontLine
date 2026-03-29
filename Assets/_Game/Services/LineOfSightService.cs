using System;
using FrontLine.Models;

namespace FrontLine.Services
{
    public class LineOfSightService
    {
        private readonly GameState _gameState;

        public LineOfSightService(GameState gameState)
        {
            _gameState = gameState;
        }

        // Returns false if any intermediate tile (start and end excluded) is Blocked.
        // TODO-POST-ALPHA: asymmetry edge case — use supercover DDA for symmetric LOS
        public bool HasLOS(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int cx = x0;
            int cy = y0;

            while (true)
            {
                if (cx == x1 && cy == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; cx += sx; }
                if (e2 < dx)  { err += dx; cy += sy; }

                if (cx == x1 && cy == y1) break; // skip end tile

                var tile = _gameState.GetTile(cx, cy);
                if (tile != null && tile.Type == TileType.Blocked)
                    return false;
            }

            return true;
        }
    }
}

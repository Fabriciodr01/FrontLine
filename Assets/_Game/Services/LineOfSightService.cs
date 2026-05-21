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

        // Supercover DDA — visits every cell the line passes through, including both
        // cells at exact corner crossings. Corner crossing blocks LOS only when both
        // adjacent cells are Blocked, guaranteeing HasLOS(A→B) == HasLOS(B→A).
        // Start and end tiles are excluded from blocking checks.
        public bool HasLOS(int x0, int y0, int x1, int y1)
        {
            int nx    = Math.Abs(x1 - x0);
            int ny    = Math.Abs(y1 - y0);
            int signX = x1 > x0 ? 1 : (x1 < x0 ? -1 : 0);
            int signY = y1 > y0 ? 1 : (y1 < y0 ? -1 : 0);

            if (nx == 0 && ny == 0) return true; // same cell

            int px = x0;
            int py = y0;

            for (int ix = 0, iy = 0; ix < nx || iy < ny; )
            {
                int decision = (1 + 2 * ix) * ny - (1 + 2 * iy) * nx;

                if (decision == 0)
                {
                    // Exact corner crossing — check both adjacent cells.
                    // Block only if BOTH are Blocked (no open passage around the corner).
                    var tileH = _gameState.GetTile(px + signX, py);
                    var tileV = _gameState.GetTile(px, py + signY);
                    bool hBlocked = tileH != null && (tileH.Type == TileType.Blocked || tileH.HasSmoke);
                    bool vBlocked = tileV != null && (tileV.Type == TileType.Blocked || tileV.HasSmoke);
                    if (hBlocked && vBlocked) return false;
                    px += signX; py += signY;
                    ix++; iy++;
                }
                else if (decision < 0)
                {
                    px += signX;
                    ix++;
                }
                else
                {
                    py += signY;
                    iy++;
                }

                if (px == x1 && py == y1) break; // skip end tile

                var tile = _gameState.GetTile(px, py);
                if (tile != null && (tile.Type == TileType.Blocked || tile.HasSmoke))
                    return false;
            }

            return true;
        }
    }
}

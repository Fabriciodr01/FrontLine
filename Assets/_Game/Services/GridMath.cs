using System.Collections.Generic;
using FrontLine.Models;

namespace FrontLine.Services
{
    public static class GridMath
    {
        // Diagonal and cardinal steps both count as 1 tile in the current alpha rules.
        public static int GetTileDistance(int fromX, int fromY, int toX, int toY)
        {
            int deltaX = System.Math.Abs(toX - fromX);
            int deltaY = System.Math.Abs(toY - fromY);
            return System.Math.Max(deltaX, deltaY);
        }

        private static readonly (int dx, int dy)[] Neighbours =
        {
            (-1, -1), (0, -1), (1, -1),
            (-1,  0),          (1,  0),
            (-1,  1), (0,  1), (1,  1),
        };

        // A* — returns the shortest tile-by-tile path from source to destination,
        // excluding the starting tile. Uses Chebyshev distance as heuristic
        // (admissible for 8-directional uniform-cost movement).
        // Tie-breaks by cross-product deviation from the straight start→goal line,
        // so paths look direct rather than zigzagging among equally-cheap routes.
        // Returns null if no path exists.
        public static List<(int x, int y)> GetPath(
            GameState gameState, int fromX, int fromY, int toX, int toY)
        {
            var gScore = new Dictionary<(int, int), int> { [(fromX, fromY)] = 0 };
            var parent = new Dictionary<(int, int), (int, int)>();
            var closed = new HashSet<(int, int)>();
            // (fScore, crossProduct tiebreaker, x, y)
            var open = new List<(int f, int cross, int x, int y)>
            {
                (GetTileDistance(fromX, fromY, toX, toY), 0, fromX, fromY)
            };

            while (open.Count > 0)
            {
                open.Sort((a, b) =>
                {
                    int fc = a.f.CompareTo(b.f);
                    return fc != 0 ? fc : a.cross.CompareTo(b.cross);
                });
                var (_, __, cx, cy) = open[0];
                open.RemoveAt(0);

                if (cx == toX && cy == toY)
                {
                    var path    = new List<(int, int)>();
                    var current = (toX, toY);
                    var start   = (fromX, fromY);
                    while (current != start)
                    {
                        path.Add(current);
                        current = parent[current];
                    }
                    path.Reverse();
                    return path;
                }

                if (!closed.Add((cx, cy))) continue;

                foreach (var (dx, dy) in Neighbours)
                {
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (closed.Contains((nx, ny))) continue;
                    var tile = gameState.GetTile(nx, ny);
                    if (tile == null || !tile.IsWalkable()) continue;

                    int tentativeG = gScore[(cx, cy)] + 1;
                    if (gScore.TryGetValue((nx, ny), out int existingG) && tentativeG >= existingG) continue;

                    gScore[(nx, ny)] = tentativeG;
                    parent[(nx, ny)] = (cx, cy);

                    int h     = GetTileDistance(nx, ny, toX, toY);
                    int cross = System.Math.Abs(
                        (nx - fromX) * (toY - fromY) - (ny - fromY) * (toX - fromX));
                    open.Add((tentativeG + h, cross, nx, ny));
                }
            }

            return null;
        }

        // BFS flood-fill — returns all walkable tiles reachable within `range` steps.
        // The starting tile is excluded from the result.
        public static HashSet<(int x, int y)> GetReachableTiles(
            GameState gameState, int fromX, int fromY, int range)
        {
            var reachable = new HashSet<(int, int)>();
            var visited   = new HashSet<(int, int)> { (fromX, fromY) };
            var queue     = new Queue<(int x, int y, int steps)>();
            queue.Enqueue((fromX, fromY, 0));

            while (queue.Count > 0)
            {
                var (x, y, steps) = queue.Dequeue();

                foreach (var (dx, dy) in Neighbours)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (visited.Contains((nx, ny))) continue;
                    visited.Add((nx, ny));

                    var tile = gameState.GetTile(nx, ny);
                    if (tile == null || !tile.IsWalkable()) continue;

                    reachable.Add((nx, ny));

                    if (steps + 1 < range)
                        queue.Enqueue((nx, ny, steps + 1));
                }
            }

            return reachable;
        }
    }
}

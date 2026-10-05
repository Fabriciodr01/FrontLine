using System.Collections.Generic;
using FrontLine.Models;

namespace FrontLine.Map.Grid
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

        // A* - returns the shortest tile-by-tile path from source to destination,
        // excluding the starting tile. Uses Chebyshev distance as heuristic
        // (admissible for 8-directional uniform-cost movement).
        // Among equally short routes, prefers progress toward the destination first,
        // then avoids moving past the target lane before reducing unnecessary turns.
        // Returns null if no path exists.
        public static List<(int x, int y)> GetPath(
            GameState gameState, int fromX, int fromY, int toX, int toY)
        {
            var gScore = new Dictionary<(int, int), int> { [(fromX, fromY)] = 0 };
            var laneScore = new Dictionary<(int, int), int> { [(fromX, fromY)] = 0 };
            var turnScore = new Dictionary<(int, int), int> { [(fromX, fromY)] = 0 };
            var crossScore = new Dictionary<(int, int), int> { [(fromX, fromY)] = 0 };
            var direction = new Dictionary<(int, int), (int dx, int dy)> { [(fromX, fromY)] = (0, 0) };
            var parent = new Dictionary<(int, int), (int, int)>();
            int startH = GetTileDistance(fromX, fromY, toX, toY);
            // (fScore, heuristic, lanePenalty, turnCount, straight-line deviation, x, y)
            var open = new List<(int f, int h, int lane, int turns, int cross, int x, int y)>
            {
                (startH, startH, 0, 0, 0, fromX, fromY)
            };

            while (open.Count > 0)
            {
                open.Sort((a, b) =>
                {
                    int fc = a.f.CompareTo(b.f);
                    if (fc != 0) return fc;

                    int hc = a.h.CompareTo(b.h);
                    if (hc != 0) return hc;

                    int lc = a.lane.CompareTo(b.lane);
                    if (lc != 0) return lc;

                    int tc = a.turns.CompareTo(b.turns);
                    if (tc != 0) return tc;

                    return a.cross.CompareTo(b.cross);
                });
                var (_, _, lane, turns, cross, cx, cy) = open[0];
                open.RemoveAt(0);

                var current = (cx, cy);
                if (laneScore[current] != lane || turnScore[current] != turns || crossScore[current] != cross)
                    continue;

                if (cx == toX && cy == toY)
                {
                    var path = new List<(int, int)>();
                    current = (toX, toY);
                    var start = (fromX, fromY);
                    while (current != start)
                    {
                        path.Add(current);
                        current = parent[current];
                    }
                    path.Reverse();
                    return path;
                }

                foreach (var (dx, dy) in Neighbours)
                {
                    int nx = cx + dx;
                    int ny = cy + dy;
                    var tile = gameState.GetTile(nx, ny);
                    if (tile == null || !tile.IsWalkable()) continue;

                    var next = (nx, ny);
                    int tentativeG = gScore[current] + 1;
                    int tentativeLane = laneScore[current] + GetLanePenalty(cx, cy, toX, toY, dx, dy);
                    int tentativeTurns = turnScore[current] + GetTurnCost(direction[current], (dx, dy));
                    int tentativeCross = GetStraightLineDeviation(fromX, fromY, toX, toY, nx, ny);
                    bool hasExisting = gScore.TryGetValue(next, out int existingG);
                    if (hasExisting)
                    {
                        int existingLane = laneScore[next];
                        int existingTurns = turnScore[next];
                        int existingCross = crossScore[next];
                        bool isBetter =
                            tentativeG < existingG ||
                            (tentativeG == existingG && tentativeLane < existingLane) ||
                            (tentativeG == existingG && tentativeLane == existingLane && tentativeTurns < existingTurns) ||
                            (tentativeG == existingG && tentativeLane == existingLane && tentativeTurns == existingTurns && tentativeCross < existingCross);

                        if (!isBetter) continue;
                    }

                    gScore[next] = tentativeG;
                    laneScore[next] = tentativeLane;
                    turnScore[next] = tentativeTurns;
                    crossScore[next] = tentativeCross;
                    direction[next] = (dx, dy);
                    parent[next] = current;

                    int h = GetTileDistance(nx, ny, toX, toY);
                    open.Add((tentativeG + h, h, tentativeLane, tentativeTurns, tentativeCross, nx, ny));
                }
            }

            return null;
        }

        private static int GetLanePenalty(int currentX, int currentY, int toX, int toY, int stepX, int stepY)
        {
            return GetAxisLanePenalty(toX - currentX, stepX)
                + GetAxisLanePenalty(toY - currentY, stepY);
        }

        private static int GetAxisLanePenalty(int remaining, int step)
        {
            if (remaining == 0)
                return step == 0 ? 0 : 1;

            if (step == 0)
                return 0;

            return System.Math.Sign(remaining) == System.Math.Sign(step) ? 0 : 1;
        }

        private static int GetTurnCost((int dx, int dy) previousDirection, (int dx, int dy) nextDirection)
        {
            return previousDirection == (0, 0) || previousDirection == nextDirection ? 0 : 1;
        }

        private static int GetStraightLineDeviation(
            int fromX, int fromY, int toX, int toY, int x, int y)
        {
            return System.Math.Abs(
                (x - fromX) * (toY - fromY) - (y - fromY) * (toX - fromX));
        }

        // BFS flood-fill - returns all walkable tiles reachable within `range` steps.
        // The starting tile is excluded from the result.
        public static HashSet<(int x, int y)> GetReachableTiles(
            GameState gameState, int fromX, int fromY, int range)
        {
            var reachable = new HashSet<(int, int)>();
            var visited = new HashSet<(int, int)> { (fromX, fromY) };
            var queue = new Queue<(int x, int y, int steps)>();
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

using System.Collections.Generic;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Map.Grid;

namespace FrontLine.Commands
{
    public class MoveCommand : ICommand
    {
        public string PlayerId { get; private set; }
        public string UnitId { get; private set; }
        public int TargetX { get; private set; }
        public int TargetY { get; private set; }
        // Populated during Execute — the tile-by-tile path the unit took.
        public List<(int x, int y)> Path { get; private set; }
        // True when the unit spent all AP to move further (dash). Preserved for future noise/mechanic hooks.
        public bool IsDash { get; private set; }

        public MoveCommand(string playerId, string unitId, int targetX, int targetY, bool isDash = false)
        {
            PlayerId = playerId;
            UnitId = unitId;
            TargetX = targetX;
            TargetY = targetY;
            IsDash = isDash;
        }

        public CommandResult Execute(GameState gameState, TurnController turnController)
        {
            // Validate turn
            if (!turnController.IsCurrentPlayer(PlayerId))
                return CommandResult.Fail($"Not {PlayerId}'s turn.");

            // Validate unit exists and belongs to player
            if (!gameState.Units.TryGetValue(UnitId, out var unit))
                return CommandResult.Fail($"Unit {UnitId} not found.");

            if (unit.OwnerId != PlayerId)
                return CommandResult.Fail($"Unit {UnitId} does not belong to {PlayerId}.");

            // Validate action points — dash requires full AP, walk requires at least 1
            int requiredAp = IsDash ? unit.MaxActionPoints : 1;
            if (unit.ActionPoints < requiredAp)
                return CommandResult.Fail($"Unit {UnitId} needs {requiredAp} AP for this move but has {unit.ActionPoints}.");

            // Validate target tile
            var targetTile = gameState.GetTile(TargetX, TargetY);
            if (targetTile == null)
                return CommandResult.Fail($"Tile ({TargetX},{TargetY}) is out of bounds.");

            if (!targetTile.IsWalkable())
                return CommandResult.Fail($"Tile ({TargetX},{TargetY}) is not walkable.");

            // Validate move range — BFS ensures the path goes through walkable tiles only
            int range = IsDash ? unit.DashRange : unit.MoveRange;
            var reachable = GridMath.GetReachableTiles(gameState, unit.TileX, unit.TileY, range);
            if (!reachable.Contains((TargetX, TargetY)))
                return CommandResult.Fail($"Target ({TargetX},{TargetY}) is not reachable within {(IsDash ? "dash" : "move")} range {range}.");

            // Compute path before state mutation (unit position still reflects origin)
            Path = GridMath.GetPath(gameState, unit.TileX, unit.TileY, TargetX, TargetY);

            // Execute — update old tile
            var oldTile = gameState.GetTile(unit.TileX, unit.TileY);
            if (oldTile != null)
            {
                oldTile.IsOccupied = false;
                oldTile.OccupyingUnitId = null;
            }

            // Update unit position
            unit.TileX = TargetX;
            unit.TileY = TargetY;

            // Update new tile
            targetTile.IsOccupied = true;
            targetTile.OccupyingUnitId = UnitId;

            // Consume action points — dash drains all AP, walk drains 1
            if (IsDash)
            {
                while (unit.ActionPoints > 0)
                    turnController.ConsumeActionPoint(UnitId);
            }
            else
            {
                turnController.ConsumeActionPoint(UnitId);
            }

            return CommandResult.Ok($"Unit {UnitId} moved to ({TargetX},{TargetY}).");
        }
    }
}

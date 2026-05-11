using System.Collections.Generic;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Services;

namespace FrontLine.Commands
{
    public class ThrowGrenadeCommand : ICommand
    {
        public string PlayerId { get; private set; }
        public string UnitId { get; private set; }
        public GrenadeType GrenadeType { get; private set; }
        public int TargetX { get; private set; }
        public int TargetY { get; private set; }

        // Populated during Execute — used by CommandProcessor to fire damage events.
        public List<string> DamagedUnitIds { get; private set; } = new List<string>();
        public List<string> KilledUnitIds { get; private set; } = new List<string>();

        private const int ThrowRange   = 4;
        private const int FragRadius   = 1;
        private const int FragDelayTurns = 1;
        private const int SmokeRadius  = 1;
        private const int SmokeDuration = 2;

        public ThrowGrenadeCommand(string playerId, string unitId, GrenadeType grenadeType, int targetX, int targetY)
        {
            PlayerId = playerId;
            UnitId = unitId;
            GrenadeType = grenadeType;
            TargetX = targetX;
            TargetY = targetY;
        }

        public CommandResult Execute(GameState gameState, TurnController turnController)
        {
            // Validate turn
            if (!turnController.IsCurrentPlayer(PlayerId))
                return CommandResult.Fail($"Not {PlayerId}'s turn.");

            // Validate attacker
            if (!gameState.Units.TryGetValue(UnitId, out var attacker))
                return CommandResult.Fail($"Unit {UnitId} not found.");

            if (attacker.OwnerId != PlayerId)
                return CommandResult.Fail($"Unit {UnitId} does not belong to {PlayerId}.");

            if (!turnController.HasActionPoints(UnitId))
                return CommandResult.Fail($"Unit {UnitId} has no action points.");

            // Validate grenade inventory
            if (this.GrenadeType == GrenadeType.Frag && attacker.FragGrenades <= 0)
                return CommandResult.Fail($"Unit {UnitId} has no frag grenades.");

            if (this.GrenadeType == GrenadeType.Smoke && attacker.SmokeGrenades <= 0)
                return CommandResult.Fail($"Unit {UnitId} has no smoke grenades.");

            // Validate target position
            if (!gameState.IsValidPosition(TargetX, TargetY))
                return CommandResult.Fail($"Target ({TargetX},{TargetY}) is out of bounds.");

            int distance = GridMath.GetTileDistance(attacker.TileX, attacker.TileY, TargetX, TargetY);
            if (distance > ThrowRange)
                return CommandResult.Fail($"Target out of throw range. Distance:{distance} Range:{ThrowRange}");

            // Execute
            if (this.GrenadeType == GrenadeType.Frag)
                ExecuteFrag(gameState, attacker);
            else
                ExecuteSmoke(gameState, attacker);

            turnController.ConsumeActionPoint(UnitId);
            return CommandResult.Ok($"{GrenadeType} grenade thrown to ({TargetX},{TargetY}).");
        }

        private void ExecuteFrag(GameState gameState, UnitData attacker)
        {
            attacker.FragGrenades--;
            gameState.PendingFragGrenades.Add(new PendingFragGrenade(TargetX, TargetY, FragDelayTurns));
        }

        private void ExecuteSmoke(GameState gameState, UnitData attacker)
        {
            attacker.SmokeGrenades--;

            for (int dx = -SmokeRadius; dx <= SmokeRadius; dx++)
            {
                for (int dy = -SmokeRadius; dy <= SmokeRadius; dy++)
                {
                    if (GridMath.GetTileDistance(0, 0, dx, dy) > SmokeRadius) continue;
                    var tile = gameState.GetTile(TargetX + dx, TargetY + dy);
                    if (tile != null)
                        tile.ApplySmoke(SmokeDuration);
                }
            }
        }
    }
}

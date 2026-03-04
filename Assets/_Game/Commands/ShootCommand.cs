using FrontLine.Models;
using FrontLine.Controllers;

namespace FrontLine.Commands
{
    public class ShootCommand : ICommand
    {
        public string PlayerId { get; private set; }
        public string UnitId { get; private set; }
        public string TargetUnitId { get; private set; }

        public ShootCommand(string playerId, string unitId, string targetUnitId)
        {
            PlayerId = playerId;
            UnitId = unitId;
            TargetUnitId = targetUnitId;
        }

        public CommandResult Execute(GameState gameState, TurnController turnController)
        {
            //TODO: temporary Guard Clause pattern, refactor later to a
            // CommandResult ValidateBasics(GameState state, TurnController turn)

            // Validate turn
            if (!turnController.IsCurrentPlayer(PlayerId))
                return CommandResult.Fail($"Not {PlayerId}'s turn.");

            // Validate attacker
            if (!gameState.Units.TryGetValue(UnitId, out var attacker))
                return CommandResult.Fail($"Attacker {UnitId} not found.");

            if (attacker.OwnerId != PlayerId)
                return CommandResult.Fail($"Unit {UnitId} does not belong to {PlayerId}.");

            if (!turnController.HasActionPoints(UnitId))
                return CommandResult.Fail($"Unit {UnitId} has no action points.");

            // Validate target
            if (!gameState.Units.TryGetValue(TargetUnitId, out var target))
                return CommandResult.Fail($"Target {TargetUnitId} not found.");

            if (target.OwnerId == PlayerId)
                return CommandResult.Fail("Cannot shoot your own unit.");

            if (!target.IsAlive)
                return CommandResult.Fail($"Target {TargetUnitId} is already dead.");

            // Validate attack range
            int distance = System.Math.Abs(target.TileX - attacker.TileX)
                         + System.Math.Abs(target.TileY - attacker.TileY);

            if (distance > attacker.AttackRange)
                return CommandResult.Fail($"Target out of attack range. Distance:{distance} Range:{attacker.AttackRange}");

            // Execute — apply damage
            target.TakeDamage(attacker.Damage);
            turnController.ConsumeActionPoint(UnitId);

            // Remove unit from game if dead
            if (!target.IsAlive)
            {
                gameState.RemoveUnit(TargetUnitId);
                return CommandResult.Ok($"Unit {UnitId} killed {TargetUnitId}.");
            }

            return CommandResult.Ok($"Unit {UnitId} dealt {attacker.Damage} damage to {TargetUnitId}. HP remaining: {target.Health}");
        }
    }
}

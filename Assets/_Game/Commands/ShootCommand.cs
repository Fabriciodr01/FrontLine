using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Services;

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
            //TODO-POST-ALPHA: temporary Guard Clause pattern, refactor later for a better validation system or abstraction

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

            // Validate attack range and line of sight
            var resolver = ServiceLocator.Instance.Get<CombatResolver>();
            var eval     = resolver.EvaluateAttack(new AttackContext(attacker, target));

            if (!eval.InRange)
                return CommandResult.Fail($"Target out of attack range. Distance:{eval.Distance} Range:{attacker.AttackRange}");

            if (!eval.HasLOS)
                return CommandResult.Fail("No line of sight.");

            // Execute — apply damage
            var combatResult = resolver.Resolve(attacker, target);

            turnController.ConsumeActionPoint(UnitId);

            if (!combatResult.Hit)
                return CommandResult.Ok($"MISS. {combatResult}");

            target.Health.TakeDamage(combatResult.DamageDealt);

            if (combatResult.Killed)
            {
                gameState.RemoveUnit(TargetUnitId);
                return CommandResult.Ok($"KILL. {combatResult}");
            }

            return CommandResult.Ok($"HIT. {combatResult}");
        }
    }
}

using System.Linq;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Commands;
using FrontLine.Services;

namespace FrontLine.AI
{
    // Attempts to shoot the nearest enemy within range and LOS from a specific unit.
    public class ShootNearestEnemyNode : IBehaviorNode
    {
        private readonly UnitData _unit;
        private readonly GameState _gameState;
        private readonly CommandProcessor _commandProcessor;
        private readonly string _aiPlayerId;
        private readonly CombatResolver _combatResolver;

        public ShootNearestEnemyNode(UnitData unit, GameState gameState, CommandProcessor commandProcessor,
            CombatResolver combatResolver, string aiPlayerId)
        {
            _unit = unit;
            _gameState = gameState;
            _commandProcessor = commandProcessor;
            _combatResolver = combatResolver;
            _aiPlayerId = aiPlayerId;
        }

        public NodeStatus Tick()
        {
            if (_unit.ActionPoints <= 0) return NodeStatus.Failure;

            var target = _gameState.Units.Values
                .Where(u => u.OwnerId != _aiPlayerId)
                .Where(u => _combatResolver.EvaluateAttack(new AttackContext(_unit, u)).CanAttack)
                .OrderBy(u => GridMath.GetTileDistance(_unit.TileX, _unit.TileY, u.TileX, u.TileY))
                .FirstOrDefault();

            if (target == null) return NodeStatus.Failure;

            var result = _commandProcessor.Process(new ShootCommand(_aiPlayerId, _unit.UnitId, target.UnitId));
            return result.Success ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    // Attempts to move a specific unit one step toward the nearest enemy.
    public class MoveTowardNearestEnemyNode : IBehaviorNode
    {
        private readonly UnitData _unit;
        private readonly GameState _gameState;
        private readonly CommandProcessor _commandProcessor;
        private readonly string _aiPlayerId;

        public MoveTowardNearestEnemyNode(UnitData unit, GameState gameState,
            CommandProcessor commandProcessor, string aiPlayerId)
        {
            _unit = unit;
            _gameState = gameState;
            _commandProcessor = commandProcessor;
            _aiPlayerId = aiPlayerId;
        }

        public NodeStatus Tick()
        {
            if (_unit.ActionPoints <= 0) return NodeStatus.Failure;

            var enemy = _gameState.Units.Values
                .Where(u => u.OwnerId != _aiPlayerId)
                .OrderBy(u => GridMath.GetTileDistance(_unit.TileX, _unit.TileY, u.TileX, u.TileY))
                .FirstOrDefault();

            if (enemy == null) return NodeStatus.Failure;

            var reachable = GridMath.GetReachableTiles(_gameState, _unit.TileX, _unit.TileY, _unit.MoveRange);
            if (reachable.Count == 0) return NodeStatus.Failure;

            var best = reachable
                .OrderBy(t => GridMath.GetTileDistance(t.x, t.y, enemy.TileX, enemy.TileY))
                .First();

            var result = _commandProcessor.Process(new MoveCommand(_aiPlayerId, _unit.UnitId, best.x, best.y, false));
            return result.Success ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}

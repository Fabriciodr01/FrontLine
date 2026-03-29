using System;
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
        private readonly LineOfSightService _losService;

        public ShootNearestEnemyNode(UnitData unit, GameState gameState, CommandProcessor commandProcessor,
            LineOfSightService losService, string aiPlayerId)
        {
            _unit = unit;
            _gameState = gameState;
            _commandProcessor = commandProcessor;
            _losService = losService;
            _aiPlayerId = aiPlayerId;
        }

        public NodeStatus Tick()
        {
            if (_unit.ActionPoints <= 0) return NodeStatus.Failure;

            var target = _gameState.Units.Values
                .Where(u => u.OwnerId != _aiPlayerId)
                .Where(u => GridMath.GetTileDistance(_unit.TileX, _unit.TileY, u.TileX, u.TileY) <= _unit.AttackRange)
                .Where(u => _losService.HasLOS(_unit.TileX, _unit.TileY, u.TileX, u.TileY))
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

            int dx = Math.Sign(enemy.TileX - _unit.TileX);
            int dy = Math.Sign(enemy.TileY - _unit.TileY);

            // Try diagonal first, then cardinal axes
            var candidates = new[]
            {
                (_unit.TileX + dx, _unit.TileY + dy),
                (_unit.TileX + dx, _unit.TileY),
                (_unit.TileX,      _unit.TileY + dy),
            };

            foreach (var (cx, cy) in candidates)
            {
                var tile = _gameState.GetTile(cx, cy);
                if (tile == null || !tile.IsWalkable()) continue;

                var result = _commandProcessor.Process(new MoveCommand(_aiPlayerId, _unit.UnitId, cx, cy));
                if (result.Success) return NodeStatus.Success;
                // fall through to next candidate if command failed despite walkable tile
            }

            return NodeStatus.Failure;
        }
    }
}

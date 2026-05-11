using System;
using FrontLine.Commands;
using FrontLine.Models;

namespace FrontLine.Controllers
{
    public class CommandProcessor
    {
        private readonly GameState _gameState;
        private readonly TurnController _turnController;

        public event Action<ICommand, CommandResult> OnCommandExecuted;
        public event Action<UnitData> OnUnitDamaged;
        public event Action<string> OnUnitKilled;
        public event Action<string> OnTurnEnded;
        public event Action<string> OnGameOver;
        public event Action<string, GrenadeBox> OnGrenadeCollected;

        public CommandProcessor(GameState gameState, TurnController turnController)
        {
            _gameState = gameState;
            _turnController = turnController;

            // Forward TurnController events through CommandProcessor
            _turnController.OnTurnEnded += playerId => OnTurnEnded?.Invoke(playerId);
            _turnController.OnGameOver += winnerId => OnGameOver?.Invoke(winnerId);
            _turnController.OnUnitDamaged += unit => OnUnitDamaged?.Invoke(unit);
            _turnController.OnUnitKilled += unitId => OnUnitKilled?.Invoke(unitId);
        }

        public CommandResult Process(ICommand command)
        {
            if (command == null)
                return CommandResult.Fail("Command is null.");

            // Snapshot unit state before execution for diffing
            UnitData targetBefore = null;
            if (command is ShootCommand shootCmd)
                _gameState.Units.TryGetValue(shootCmd.TargetUnitId, out targetBefore);

            // Execute
            var result = command.Execute(_gameState, _turnController);

            // Fire events based on what happened
            OnCommandExecuted?.Invoke(command, result);

            if (result.Success)
                HandleSideEffects(command, targetBefore);

            if (!result.Success)
                UnityEngine.Debug.LogWarning($"[CommandProcessor] Failed: {result.Message}");

            return result;
        }

        private void HandleSideEffects(ICommand command, UnitData targetBefore)
        {
            switch (command)
            {
                case ShootCommand shoot:
                    HandleShootSideEffects(shoot, targetBefore);
                    break;

                case MoveCommand move:
                    HandleMoveSideEffects(move);
                    break;

                case ThrowGrenadeCommand throwCmd:
                    HandleThrowSideEffects(throwCmd);
                    break;

                case EndTurnCommand endTurn:
                    // TurnController already fired OnTurnEnded/OnGameOver
                    break;
            }
        }

        private void HandleShootSideEffects(ShootCommand shoot, UnitData targetBefore)
        {
            if (targetBefore == null) return;

            // Unit still alive — just damaged
            if (_gameState.Units.TryGetValue(shoot.TargetUnitId, out var targetAfter))
            {
                OnUnitDamaged?.Invoke(targetAfter);
                return;
            }

            // Unit no longer in GameState — it was killed
            OnUnitKilled?.Invoke(shoot.TargetUnitId);
        }

        private void HandleMoveSideEffects(MoveCommand move)
        {
            if (!_gameState.Units.TryGetValue(move.UnitId, out var unit)) return;
            if (!_gameState.GrenadeBoxes.TryGetValue((move.TargetX, move.TargetY), out var box)) return;

            // Auto-collect grenade box on landing (free action — matches XCOM/Gears Tactics convention)
            if (box.GrenadeType == GrenadeType.Frag)
                unit.FragGrenades++;
            else
                unit.SmokeGrenades++;

            _gameState.RemoveGrenadeBox(move.TargetX, move.TargetY);
            OnGrenadeCollected?.Invoke(move.UnitId, box);
        }

        private void HandleThrowSideEffects(ThrowGrenadeCommand throwCmd)
        {
            foreach (var unitId in throwCmd.KilledUnitIds)
                OnUnitKilled?.Invoke(unitId);

            foreach (var unitId in throwCmd.DamagedUnitIds)
                if (_gameState.Units.TryGetValue(unitId, out var unit))
                    OnUnitDamaged?.Invoke(unit);
        }
    }
}

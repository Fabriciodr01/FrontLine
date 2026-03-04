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

        public CommandProcessor(GameState gameState, TurnController turnController)
        {
            _gameState = gameState;
            _turnController = turnController;

            // Forward TurnController events through CommandProcessor
            _turnController.OnTurnEnded += playerId => OnTurnEnded?.Invoke(playerId);
            _turnController.OnGameOver += winnerId => OnGameOver?.Invoke(winnerId);
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

                case EndTurnCommand endTurn:
                    // TurnController already fired OnTurnEnded/OnGameOver
                    // Nothing extra needed here
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
    }
}

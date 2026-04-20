using System;
using System.Linq;
using FrontLine.Models;

namespace FrontLine.Controllers
{
    public class TurnController
    {
        private readonly GameState _gameState;
        private readonly GameStateMachine _stateMachine;

        public string CurrentPlayerId { get; private set; }
        public int TurnNumber => _gameState.TurnNumber;

        public event Action<string> OnTurnStarted;
        public event Action<string> OnTurnEnded;
        public event Action<string> OnGameOver;

        public TurnController(GameState gameState, GameStateMachine stateMachine)
        {
            _gameState = gameState;
            _stateMachine = stateMachine;
        }

        public void StartGame()
        {
            TransitionTo(GamePhase.Setup);
            TransitionTo(GamePhase.Player1Turn);
            CurrentPlayerId = "Player1";
            _gameState.TurnNumber = 1;
            _gameState.WinnerPlayerId = null;
            ResetActionPoints(CurrentPlayerId);
            OnTurnStarted?.Invoke(CurrentPlayerId);
        }

        public void EndTurn(string requestingPlayerId)
        {
            if (requestingPlayerId != CurrentPlayerId)
            {
                UnityEngine.Debug.LogWarning(
                    $"[TurnController] {requestingPlayerId} tried to end turn but it's {CurrentPlayerId}'s turn.");
                return;
            }

            OnTurnEnded?.Invoke(CurrentPlayerId);

            if (CheckWinCondition(out string winnerId))
            {
                _gameState.WinnerPlayerId = winnerId;
                TransitionTo(GamePhase.GameOver);
                OnGameOver?.Invoke(winnerId);
                return;
            }

            SwitchTurn();
        }

        public bool IsCurrentPlayer(string playerId) => playerId == CurrentPlayerId;

        public bool HasActionPoints(string unitId)
        {
            if (!_gameState.Units.TryGetValue(unitId, out var unit))
                return false;
            return unit.ActionPoints > 0;
        }

        public void ConsumeActionPoint(string unitId)
        {
            if (!_gameState.Units.TryGetValue(unitId, out var unit))
                return;
            unit.ActionPoints = Math.Max(0, unit.ActionPoints - 1);
        }

        private void SwitchTurn()
        {
            if (CurrentPlayerId == "Player1")
            {
                TransitionTo(GamePhase.Player2Turn);
                CurrentPlayerId = "Player2";
            }
            else
            {
                TransitionTo(GamePhase.Player1Turn);
                CurrentPlayerId = "Player1";
                _gameState.TurnNumber++;
            }

            TickSmoke();
            ResetActionPoints(CurrentPlayerId);
            OnTurnStarted?.Invoke(CurrentPlayerId);
        }

        private void TickSmoke()
        {
            for (int x = 0; x < _gameState.GridWidth; x++)
                for (int y = 0; y < _gameState.GridHeight; y++)
                    _gameState.Grid[x, y].TickSmoke();
        }

        private void ResetActionPoints(string playerId)
        {
            foreach (var unit in _gameState.Units.Values.Where(u => u.OwnerId == playerId))
                unit.ResetActionPoints();
        }

        private void TransitionTo(GamePhase phase)
        {
            if (_stateMachine.TryTransition(phase))
                _gameState.CurrentPhase = phase;
        }

        private bool CheckWinCondition(out string winnerId)
        {
            bool player1HasUnits = _gameState.Units.Values.Any(u => u.OwnerId == "Player1" && u.IsAlive);
            bool player2HasUnits = _gameState.Units.Values.Any(u => u.OwnerId == "Player2" && u.IsAlive);

            if (!player1HasUnits)
            {
                winnerId = "Player2";
                return true;
            }

            if (!player2HasUnits)
            {
                winnerId = "Player1";
                return true;
            }

            winnerId = null;
            return false;
        }
    }
}

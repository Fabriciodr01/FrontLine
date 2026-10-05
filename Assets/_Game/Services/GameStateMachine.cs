using System;
using System.Collections.Generic;
using FrontLine.Models;

namespace FrontLine.Controllers
{
    public class GameStateMachine
    {
        private GamePhase _currentPhase;
        private readonly Dictionary<GamePhase, List<GamePhase>> _validTransitions;

        public GamePhase CurrentPhase => _currentPhase;

        public event Action<GamePhase, GamePhase> OnPhaseChanged;

        public GameStateMachine()
        {
            _currentPhase = GamePhase.WaitingForPlayers;
            _validTransitions = new Dictionary<GamePhase, List<GamePhase>>
            {
                { GamePhase.WaitingForPlayers, new List<GamePhase> { GamePhase.Setup } },
                { GamePhase.Setup,             new List<GamePhase> { GamePhase.Player1Turn } },
                { GamePhase.Player1Turn,       new List<GamePhase> { GamePhase.Player2Turn, GamePhase.GameOver } },
                { GamePhase.Player2Turn,       new List<GamePhase> { GamePhase.Player1Turn, GamePhase.GameOver } },
                { GamePhase.GameOver,          new List<GamePhase>() }
            };
        }

        public bool TryTransition(GamePhase targetPhase)
        {
            if (!_validTransitions.TryGetValue(_currentPhase, out var allowed))
                return false;

            if (!allowed.Contains(targetPhase))
            {
                UnityEngine.Debug.LogWarning(
                    $"[GameStateMachine] Invalid transition: {_currentPhase} → {targetPhase}");
                return false;
            }

            var previous = _currentPhase;
            _currentPhase = targetPhase;
            OnPhaseChanged?.Invoke(previous, _currentPhase);
            return true;
        }

        public bool IsPlayerTurn(string playerId)
        {
            return playerId == "Player1" && _currentPhase == GamePhase.Player1Turn
                || playerId == "Player2" && _currentPhase == GamePhase.Player2Turn;
        }

        public override string ToString() => $"[GameStateMachine] Phase: {_currentPhase}";
    }
}

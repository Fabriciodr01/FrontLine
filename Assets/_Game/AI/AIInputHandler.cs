using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Commands;
using FrontLine.Services;
using FrontLine.Input;

namespace FrontLine.AI
{
    // Orchestrates the AI turn: sequences each unit's AIUnitAgent, then ends the turn.
    // MonoBehaviour because it needs StartCoroutine.
    // Decision logic lives entirely inside AIUnitAgent and its BT — this class only
    // controls timing and sequencing.
    public class AIInputHandler : MonoBehaviour, IInputHandler
    {
        private const string AiPlayerId    = "Player2";
        private const float  StartDelay    = 0.5f;
        private const float  ActionDelay   = 0.4f;
        private const int    MaxActionsPerUnit = 10;

        private GameState         _gameState;
        private CommandProcessor  _commandProcessor;
        private CombatResolver    _combatResolver;
        private Coroutine         _coroutine;

        public void Initialize()
        {
            _gameState        = ServiceLocator.Instance.Get<GameState>();
            _commandProcessor = ServiceLocator.Instance.Get<CommandProcessor>();
            _combatResolver   = ServiceLocator.Instance.Get<CombatResolver>();
        }

        public void OnTurnStarted(string playerId)
        {
            if (playerId != AiPlayerId) return;

            // Build agents fresh each turn — dead units are already removed from GameState
            var agents = _gameState.Units.Values
                .Where(u => u.OwnerId == AiPlayerId)
                .Select(u => new AIUnitAgent(u, _gameState, _commandProcessor, _combatResolver, AiPlayerId))
                .ToList();

            _coroutine = StartCoroutine(RunAITurn(agents));
        }

        public void OnTurnEnded(string playerId)
        {
            if (playerId != AiPlayerId) return;
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        private IEnumerator RunAITurn(List<AIUnitAgent> agents)
        {
            yield return new WaitForSeconds(StartDelay);

            foreach (var agent in agents)
            {
                int safety = MaxActionsPerUnit;
                while (agent.Unit.ActionPoints > 0 && safety-- > 0)
                {
                    var status = agent.Tick();
                    if (status == NodeStatus.Failure) break;
                    yield return new WaitForSeconds(ActionDelay);
                }
            }

            _commandProcessor.Process(new EndTurnCommand(AiPlayerId));
        }
    }
}

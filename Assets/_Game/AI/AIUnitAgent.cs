using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Services;

namespace FrontLine.AI
{
    // Autonomous decision-maker for a single AI-controlled unit.
    // Owns the unit's behavior tree — the orchestrator (AIInputHandler) calls Tick()
    // each action step and interprets the result, but has no knowledge of what
    // the tree does internally.
    public class AIUnitAgent
    {
        public UnitData Unit { get; }
        private readonly IBehaviorNode _bt;

        public AIUnitAgent(UnitData unit, GameState gameState, CommandProcessor commandProcessor,
            LineOfSightService losService, string aiPlayerId)
        {
            Unit = unit;
            _bt = new SelectorNode(
                new ShootNearestEnemyNode(unit, gameState, commandProcessor, losService, aiPlayerId),
                new MoveTowardNearestEnemyNode(unit, gameState, commandProcessor, aiPlayerId)
            );
        }

        // Returns Success if an action was taken this tick, Failure if the unit cannot act.
        public NodeStatus Tick() => _bt.Tick();
    }
}

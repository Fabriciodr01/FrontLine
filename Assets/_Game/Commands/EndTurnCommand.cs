using FrontLine.Models;
using FrontLine.Controllers;

namespace FrontLine.Commands
{
    public class EndTurnCommand : ICommand
    {
        public string PlayerId { get; private set; }
        public string UnitId => null;

        public EndTurnCommand(string playerId)
        {
            PlayerId = playerId;
        }

        public CommandResult Execute(GameState gameState, TurnController turnController)
        {
            if (!turnController.IsCurrentPlayer(PlayerId))
                return CommandResult.Fail($"Not {PlayerId}'s turn.");

            turnController.EndTurn(PlayerId);
            return CommandResult.Ok($"{PlayerId} ended their turn.");
        }
    }
}

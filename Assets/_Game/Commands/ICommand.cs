namespace FrontLine.Commands
{
    public interface ICommand
    {
        string PlayerId { get; }
        string UnitId { get; }

        CommandResult Execute(
            FrontLine.Models.GameState gameState,
            FrontLine.Controllers.TurnController turnController);
    }

    public class CommandResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }

        public static CommandResult Ok(string message = "")
            => new CommandResult { Success = true, Message = message };

        public static CommandResult Fail(string message)
            => new CommandResult { Success = false, Message = message };
    }
}

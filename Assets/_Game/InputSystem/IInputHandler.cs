namespace FrontLine.InputSystem
{
    public interface IInputHandler
    {
        void OnTurnStarted(string playerId);
        void OnTurnEnded(string playerId);
    }
}

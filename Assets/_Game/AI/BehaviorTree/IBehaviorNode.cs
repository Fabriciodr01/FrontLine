namespace FrontLine.AI
{
    public enum NodeStatus { Success, Failure, Running }

    public interface IBehaviorNode
    {
        NodeStatus Tick();
    }
}

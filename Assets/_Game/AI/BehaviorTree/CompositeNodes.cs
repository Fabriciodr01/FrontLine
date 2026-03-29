namespace FrontLine.AI
{
    // Tries children in order, returns Success on the first child that succeeds.
    public class SelectorNode : IBehaviorNode
    {
        private readonly IBehaviorNode[] _children;

        public SelectorNode(params IBehaviorNode[] children)
        {
            _children = children;
        }

        public NodeStatus Tick()
        {
            foreach (var child in _children)
            {
                var status = child.Tick();
                if (status == NodeStatus.Success) return NodeStatus.Success;
                if (status == NodeStatus.Running)  return NodeStatus.Running;
            }
            return NodeStatus.Failure;
        }
    }

    // Runs children in order, stops on first failure.
    public class SequenceNode : IBehaviorNode
    {
        private readonly IBehaviorNode[] _children;

        public SequenceNode(params IBehaviorNode[] children)
        {
            _children = children;
        }

        public NodeStatus Tick()
        {
            foreach (var child in _children)
            {
                var status = child.Tick();
                if (status == NodeStatus.Failure) return NodeStatus.Failure;
                if (status == NodeStatus.Running) return NodeStatus.Running;
            }
            return NodeStatus.Success;
        }
    }
}

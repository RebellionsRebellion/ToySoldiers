using UnityEngine.AI;

public interface IState
{
    void Execute();
}

public abstract class AIState : IState
{
    protected AIStateMachine controller;
    protected NavMeshAgent agent;

    public AIState(AIStateMachine controller, NavMeshAgent agent)
    {
        this.controller = controller;
        this.agent = agent;
    }
    
    public abstract void Execute();
}

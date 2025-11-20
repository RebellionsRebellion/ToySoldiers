using UnityEngine.AI;

public class DeathState : AIState
{
    public DeathState(AIStateMachine controller, NavMeshAgent agent) : base(controller, agent)
    {
        
    }
    
    public override void Execute()
    {
        agent.isStopped = true;
    }
}

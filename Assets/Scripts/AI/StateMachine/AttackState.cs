using UnityEngine;
using UnityEngine.AI;

public class AttackState : AIState
{
    private Transform player;

    public AttackState(AIStateMachine controller, NavMeshAgent agent, Transform player) : base(controller, agent)
    {
        this.player = player;
    }

    // Moves towards player at them moment, will update with actual enemy logic eventually
    public override void Execute()
    {
        agent.SetDestination(player.position);
    }
}

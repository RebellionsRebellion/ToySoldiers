using UnityEngine;
using UnityEngine.AI;

public class AttackState : AIState
{
    private Transform player;
    private AIWeaponSystem weaponSystem;
    private float stoppingDistance = 10f;

    public AttackState(AIStateMachine controller, NavMeshAgent agent, Transform player) : base(controller, agent)
    {
        this.player = player;
        weaponSystem = controller.GetComponentInChildren<AIWeaponSystem>();
    }

    // Moves towards player at them moment, will update with actual enemy logic eventually
    public override void Execute()
    {
        agent.SetDestination(player.position);
        if (agent.remainingDistance <= stoppingDistance)
        {
            agent.isStopped = true;
            weaponSystem.target = player;
            weaponSystem.Fire();
        }
        else
        {
            agent.isStopped = false;
        }
    }
}

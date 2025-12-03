using UnityEngine;
using UnityEngine.AI;

public class AttackState : AIState
{
    private Transform player;
    private AIWeaponSystem weaponSystem;
    private float stoppingDistance = 10f;
    private CoverPoint coverPoint;
    private bool squadAlerted = false;

    public AttackState(AIStateMachine controller, NavMeshAgent agent, Transform player) : base(controller, agent)
    {
        this.player = player;
        weaponSystem = controller.GetComponentInChildren<AIWeaponSystem>();
    }

    // Moves towards player at them moment, will update with actual enemy logic eventually
    public override void Execute()
    {
        if (!squadAlerted)
        {
            AlertSquad();
            squadAlerted = true;
        }
        
        if (coverPoint == null)
        {
            coverPoint = CoverPointManager.instance.GetNearestCoverPoint(controller.transform.position, player);

            if (coverPoint != null)
            {
                coverPoint.TakeCoverPoint(controller);
                controller.ChangeState(new MoveToCoverState(controller, agent, coverPoint, player));
                return;
            }
        }
        
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
    
    private void AlertSquad()
    {
        if (controller.commander != null)
        {
            CommanderController commanderController = controller.commander.GetComponent<CommanderController>();
            if (commanderController != null)
            {
                AIStateMachine commanderAI = commanderController.GetComponent<AIStateMachine>();
                if (commanderAI != null && !(commanderAI.CurrentState is AttackState) && !(commanderAI.CurrentState is MoveToCoverState) && !(commanderAI.CurrentState is BehindCoverState) && !(commanderAI.CurrentState is PeekShootState))
                {
                    commanderAI.ChangeState(new AttackState(commanderAI, commanderAI.agent, player));
                }
                
                foreach (AIStateMachine follower in commanderController.Followers)
                {
                    if (follower != null && !(follower.CurrentState is AttackState) && !(follower.CurrentState is MoveToCoverState) && !(follower.CurrentState is BehindCoverState) && !(follower.CurrentState is PeekShootState))
                    {
                        follower.ChangeState(new AttackState(follower, follower.agent, player));
                    }
                }
            }
        }
        
        CommanderController selfCommander = controller.GetComponent<CommanderController>();
        if (selfCommander != null)
        {
            foreach (AIStateMachine follower in selfCommander.Followers)
            {
                if (follower != null && !(follower.CurrentState is AttackState) && !(follower.CurrentState is MoveToCoverState) && !(follower.CurrentState is BehindCoverState) && !(follower.CurrentState is PeekShootState))
                {
                    follower.ChangeState(new AttackState(follower, follower.agent, player));
                }
            }
        }
    }
}

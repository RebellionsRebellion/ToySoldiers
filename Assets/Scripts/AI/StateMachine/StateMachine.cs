using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIStateMachine : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent agent;
    private AIState currentState;
    
    [Tooltip("Set waypoints by creating empty game objects in the scene, then setting their transform to the waypoint.")]
    [SerializeField] private List<Transform> waypoints;
    public Transform commander;
    public Vector2 formationOffset;
    [HideInInspector] public int currentWaypoint; 
    [HideInInspector] public AIVision vision;

    // Sets starting states for AI 
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<AIVision>();
        ReturnToStartingState();
    }

    void Update()
    {
        currentState?.Execute();
        // if player enters vision collider, switch to attack state
        if (vision.canSeePlayer && !(currentState is AttackState))
        { 
            ChangeState(new AttackState(this, agent, vision.player));
        }
        // else if cant see player and is in attack state, switch to search state
        else if (!vision.canSeePlayer && currentState is AttackState)
        {
            ChangeState(new SearchState(this, agent, vision.lastSeenPosition));
        }
    }

    // function to change state
    public void ChangeState(AIState newState)
    {
        if (newState is PatrolState)
        {
            CommanderController commander = GetComponent<CommanderController>();
            commander.hasGroupedUp = false;
        }
        currentState = newState;
    }
    
    // When AI dies, it changes state and if it was a commander a new one is set, or if just a follower then it is removed from commanders list
    public void Die()
    {
        ChangeState(new DeathState(this, agent));
        CommanderController commanderController = GetComponent<CommanderController>();
        if (commanderController != null)
        {
            foreach (AIStateMachine follower in commanderController.Followers)
            {
                if (follower != null)
                {
                    follower.commander = null;
                }
            }
            commanderController.PromoteNewCommander();
        }
         
        if (commander != null)
        {
            CommanderController followerCommander = commander.GetComponent<CommanderController>();
            if (commander != null)
            {
                followerCommander.Followers.Remove(this);
            }
        }

        Destroy(gameObject, 2f);
    }

    public void ReturnToStartingState()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            ChangeState(new PatrolState(this, agent, waypoints, currentWaypoint));
        }
        else if (commander != null)
        {
            ChangeState(new FollowCommanderState(this, agent, commander, formationOffset));
        }
    }

    public List<Transform> Waypoints
    {
        get { return waypoints; }
        set { waypoints = value; }
    }
    
}

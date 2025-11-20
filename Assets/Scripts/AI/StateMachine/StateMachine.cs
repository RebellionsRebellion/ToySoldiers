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

    // Sets starting states for AI 
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (waypoints != null && waypoints.Count > 0)
        {
            ChangeState(new PatrolState(this, agent, waypoints, currentWaypoint));
        }
        else if (commander != null)
        {
            ChangeState(new FollowCommanderState(this, agent, commander, formationOffset));
        }
    }

    void Update()
    {
        currentState?.Execute();
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

    // if player enters vision collider, switch to attack state
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !(currentState is AttackState))
        {
            ChangeState(new AttackState(this, agent, other.transform));
        }
    }

    // if player leaves vision collider, switch to Patrol State if commander, otherwise follow commander state
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && currentState is AttackState)
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

    public List<Transform> Waypoints
    {
        get { return waypoints; }
        set { waypoints = value; }
    }
    
}

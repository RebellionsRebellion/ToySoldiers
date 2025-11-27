using UnityEngine;

public class AIVision : MonoBehaviour
{
    [Tooltip("Distance the enemy can See")]
    [SerializeField] private float Range;
    
    [Tooltip("Enemies field of view")]
    [SerializeField] private float FOV;
    
    [HideInInspector] public bool canSeePlayer = false;
    [HideInInspector] public Transform player;
    private SphereCollider visionCollider;
    [SerializeField] private LayerMask visionMask;
    
    [Tooltip("Time before the player is seen inside vision")]
    [SerializeField] private float aggressionTime = 1.0f;
    private float visibleTimer = 0f;
    
    [HideInInspector] public Vector3 lastSeenPosition;

    void Start()
    {
        visionCollider = GetComponent<SphereCollider>();
        visionCollider.radius = Range;
        // Finds the player object using Tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Will activate when player is inside the trigger collider
    void OnTriggerStay(Collider other)
    {
        // if the object inside trigger is not player then end early
        if (!other.CompareTag("Player"))
        {
            return;
        }
        // Calculates direction to the player
        Vector3 direction = (player.position - transform.position).normalized;
        // Which is then used to calculate the angle between the AI forwards and the players direction
        float angle = Vector3.Angle(transform.forward, direction);
        // If the angle calculated is larger than half the FOV, then the player is outside of its vision cone
        if (angle > FOV * 0.5f)
        {
            visibleTimer = 0f;
            canSeePlayer = false;
            return;
        }
        // Does raycast to check if anything is blocking vision between enemy and player
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, direction, out RaycastHit hit,  Range,  visionMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // When player has been inside vision for the aggression time, AI can see the player.
                visibleTimer += Time.deltaTime;
                if (visibleTimer >= aggressionTime)
                {
                    canSeePlayer = true;
                    lastSeenPosition = player.position;
                }
                return;
            }
        }
        visibleTimer = 0f;
        canSeePlayer = false;
    }

    // Will activate when player Leaves the trigger collider
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            visibleTimer = 0f;
            canSeePlayer = false;
        }
    }
}

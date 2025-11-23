using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [HideInInspector] public float currentHealth;
    private AIStateMachine stateMachine;
    [SerializeField] private Animator aiAnimator;
    public Animator AIAnimator => aiAnimator;
    private NavMeshAgent navMeshAgent;
    private static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        stateMachine = GetComponent<AIStateMachine>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Walking animation based on current speed
        float horizontalSpeed = navMeshAgent.velocity.magnitude;
        // Divide by max speed to get 0-1 range
        horizontalSpeed /= navMeshAgent.speed;
        // Half to fit walking blend tree
        horizontalSpeed /= 2f;
        // If sprinting, remove multiplier and instead double to get full speed
        if (navMeshAgent.speed > 2)
        {
            horizontalSpeed *= 2f;
        }
        aiAnimator.SetFloat(AnimMoveSpeed, horizontalSpeed , 0.2f, Time.deltaTime);
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            stateMachine.Die();
        }
    }
}

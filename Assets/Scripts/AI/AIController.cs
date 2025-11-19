using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [HideInInspector] public float currentHealth;
    private AIStateMachine stateMachine;

    void Start()
    {
        stateMachine = GetComponent<AIStateMachine>();
        currentHealth = maxHealth;
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

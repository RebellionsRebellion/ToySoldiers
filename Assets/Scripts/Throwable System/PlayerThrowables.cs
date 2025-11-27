using UnityEngine;

public class PlayerThrowables : MonoBehaviour
{
    [SerializeField] private PlayerInputController playerInputController;
    [SerializeField] private ThrowableSpawner spawner;
    public ThrowableDataSO currentThrowable;

    void OnEnable()
    {
        playerInputController.OnThrowAction += ThrowThing;
    }
    
    void OnDisable()
    {
        playerInputController.OnThrowAction -= ThrowThing;
    }

    void ThrowThing()
    {
        spawner.ThrowObject(currentThrowable);
    }
}

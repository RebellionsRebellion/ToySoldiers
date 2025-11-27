using System;
using UnityEngine;

public class PlayerThrowables : MonoBehaviour
{
    [SerializeField] private PlayerInputController playerInputController;
    [SerializeField] private ThrowableSpawner spawner;
    private ThrowableDataSO currentThrowable;
    
    private PlayerInventory playerInventory => PlayerInventory.Instance;


    void OnEnable()
    {
        playerInputController.OnThrowAction += ThrowThing;
    }
    
    void OnDisable()
    {
        playerInputController.OnThrowAction -= ThrowThing;
    }

    private void Start()
    {
        currentThrowable = playerInventory.GetStartingThrowable();
    }

    void ThrowThing()
    {
        if (playerInventory.GetThrowableCount() > 0)
        {
            spawner.ThrowObject(currentThrowable);
            playerInventory.AdjustThrowableCount(-1);
        }
    }
}

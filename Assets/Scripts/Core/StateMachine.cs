using System.Collections;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State CurrentState => currentState;
    protected State currentState;
    
    protected virtual void Update()
    {
        currentState?.CheckTransitions(); 
        currentState?.Tick();
    }

    private void FixedUpdate()
    {
        currentState?.FixedTick();
    }

    public bool SwitchState(State newState, State oldState)
    {
        if (!newState.CanEnter())
            return false;
        
        
        // Avoid switching states multiple times in one frame
        if (oldState != null)
        {
            if (oldState.HasChangedState)
                return false;
            oldState.HasChangedState = true;
            StartCoroutine(ResetChangedStateFlagCoroutine(oldState));
        }

        
        currentState?.OnExit();

        currentState = newState;
        currentState?.OnEnter();
        
        return true;
    }
    
    private IEnumerator ResetChangedStateFlagCoroutine(State oldState)
    {
        yield return new WaitForEndOfFrame();
        oldState.HasChangedState = false;
    }
}

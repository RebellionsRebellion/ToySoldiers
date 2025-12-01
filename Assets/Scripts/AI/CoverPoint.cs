using UnityEngine;

public class CoverPoint : MonoBehaviour
{
    // is cover point taken?
    [HideInInspector] public bool isTaken = false;
    [HideInInspector] public AIStateMachine aiStateMachine = null;

    void Start()
    {
        CoverPointManager.instance.AddCoverPoint(this);
    }

    private void OnDestroy()
    {
        if (CoverPointManager.instance != null)
        {
            CoverPointManager.instance.RemoveCoverPoint(this);
        }
    }
    
    // claims cover for an enemy
    public bool TakeCoverPoint(AIStateMachine ai)
    {
        if (isTaken)
        {
            return false;
        }
        isTaken = true;
        aiStateMachine = ai;
        return true;
    }

    public void LeaveCoverPoint()
    {
        isTaken = false;
        aiStateMachine = null;
    }
}

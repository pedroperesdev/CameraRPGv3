using UnityEngine;
public abstract class TallState
{
    protected Tall TallInstance;
    public TallState(Tall enemyInstance)
    {
        TallInstance = enemyInstance;
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();

}
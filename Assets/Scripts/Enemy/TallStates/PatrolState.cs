using System.Collections;
using UnityEngine;

public class PatrolState : TallState
{
    private float waitingTime = 4f;
    private float currentWaitingTime;
    private Transform currentDest;
    public PatrolState(Tall enemyInstance) : base(enemyInstance) {}
    public override void EnterState()
    {
        currentWaitingTime = 0f;
        currentDest = TallInstance.PatrolStart;
        TallInstance.NMA.SetDestination(currentDest.position);
    }
    public override void UpdateState()
    {
        //change to alerted if LOS + dist to player
        if(TallInstance.IsAlerted()) {
            TallInstance.ChangeState(TallInstance.Alerted);
            return;
        }
        if(!TallInstance.NMA.pathPending && TallInstance.NMA.remainingDistance <= TallInstance.NMA.stoppingDistance)
        {
            if(currentWaitingTime >= waitingTime)
            {
                ChangeDestination();
                currentWaitingTime = 0f;
                TallInstance.NMA.SetDestination(currentDest.position);
                return;
            }
            currentWaitingTime += Time.deltaTime;
        }
    }

    public override void ExitState() {}

    void ChangeDestination()
    {
        if(currentDest == TallInstance.PatrolStart) currentDest = TallInstance.PatrolEnd;
        else currentDest = TallInstance.PatrolStart;
    }

}
using System.Collections;
using UnityEngine;

public class AlertedState : TallState
{
    public AlertedState(Tall enemyInstance) : base(enemyInstance) {}
    public override void EnterState()
    {
        TallInstance.NMA.enabled = false;
        Debug.Log("alerted");
        TallInstance.JumpOnce();
        TallInstance.WaitAndStartChase();
    }
    public override void UpdateState()
    {
        TallInstance.LookAtPlayer();
    }
    public override void ExitState()
    {
        TallInstance.NMA.enabled = true;
    }
}
public class ChaseState : TallState
{
    public ChaseState(Tall enemyInstance) : base(enemyInstance) {}
    public override void EnterState()
    {
        TallInstance.SetChaseSpeed();
    }
    public override void UpdateState()
    {
        TallInstance.NMA.SetDestination(PlayerController.PlayerTransform.position);
        TallInstance.TryAttackPlayer();
    }
    public override void ExitState(){}
}
public class IdleState : PlayerMovementState
{
    private bool stoppingWobble;
    public IdleState(PlayerController pc) : base(pc) { }
    public override void EnterState()
    {
        stoppingWobble = true;
        PC.WobbleElapsed = 0f;
    }
    public override void UpdateState()
    {
        if (PC.CrouchAction.IsPressed() && !PC.Aiming && PC.CC.isGrounded)
        {
            PC.ChangeMovementState(PC.Crouch);
            return;
        }
        if (PC.MoveHeld)
        {
            PC.ChangeMovementState(PC.Walk);
            return;
        }
        if (stoppingWobble)
        {
            stoppingWobble = StopWobble();
        }
        PC.RegenStamina();
    }
    public override void ExitState(){}

}

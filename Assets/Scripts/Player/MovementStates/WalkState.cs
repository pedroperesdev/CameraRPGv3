public class WalkState : PlayerMovementState
{
    protected float CurrentMovSpeed = 5f;
    private float wobbleFreqWalk = 0.8f;
    private float wobbleAmp = 0.08f;

    public WalkState(PlayerController pc) : base(pc) { }
    public override void EnterState(){}
    public override void UpdateState()
    {
        if (!PC.MoveHeld)
        {
            PC.ChangeMovementState(PC.Idle);
            return;
        }
        if (PC.SprintAction.IsPressed() && PC.CanStartSprint)
        {
            PC.ChangeMovementState(PC.Run);
            return;
        }
        if (PC.CrouchAction.IsPressed() && !PC.Aiming && PC.CC.isGrounded)
        {
            PC.ChangeMovementState(PC.Crouch);
            return;
        }
        MoveHorizontal(CurrentMovSpeed, wobbleFreqWalk, wobbleAmp);
        PC.RegenStamina();
    }
    public override void ExitState(){}

}

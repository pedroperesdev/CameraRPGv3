public class RunState : PlayerMovementState
{
    protected float CurrentMovSpeed = 8f;
    float wobbleFreqSprint = 1.5f;
    private float wobbleAmp = 0.08f;
    public RunState(PlayerController pc) : base(pc) { }
    public override void EnterState(){}
    public override void UpdateState()
    {
        if (!PC.SprintAction.IsPressed() || PC.Aiming || !PC.CC.isGrounded)
        {
            PC.StopSprintAndEnterRecovery();
            PC.ChangeMovementState(PC.MoveHeld ? PC.Walk : PC.Idle);
            return;
        }
        if (!PC.MoveHeld)
        {
            PC.StopSprintAndEnterRecovery();
            PC.ChangeMovementState(PC.Idle);
            return;
        }
        MoveHorizontal(CurrentMovSpeed, wobbleFreqSprint, wobbleAmp);
        //Out of stamina, needs to release sprint and recover before sprinting again
        if (!PC.DrainStamina())
        {
            PC.ChangeMovementState(PC.Walk);
        }
    }
    public override void ExitState(){}

}

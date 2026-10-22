public class CrouchState : PlayerMovementState
{
    protected float CurrentMovSpeed = 3f;
    float wobbleFreqCrouch = 0f;
    private float wobbleAmp = 0f;
    public CrouchState(PlayerController pc) : base(pc) { }
    public override void EnterState()
    {
        PC.ChangeHitbox(PlayerController.HitboxState.Crouch);
    }
    public override void UpdateState()
    {
        if (!PC.CrouchAction.IsPressed())
        {
            PC.ChangeMovementState(PC.MoveHeld ? PC.Walk : PC.Idle);
            return;
        }
        MoveHorizontal(CurrentMovSpeed, wobbleFreqCrouch, wobbleAmp);
        PC.RegenStamina();
    }
    public override void ExitState()
    {
        PC.ChangeHitbox(PlayerController.HitboxState.Normal);
    }

}

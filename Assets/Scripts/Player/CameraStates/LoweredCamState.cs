public class LoweredCamState : PlayerCameraState
{
    public LoweredCamState(PlayerController pc) : base(pc) { }
    public override void EnterState()
    {
        PC.CurrAnimState = PlayerController.AnimationStates.StopAim;
        PC.SetPlayerHUD();
    }
    public override void Tick()
    {
        if (PC.AimAction.WasPressedThisFrame())
        {
            PC.ChangeCameraState(PC.Aim);
            return;
        }
        PC.CheckForInteraction();
        PC.CheckForSlotChange();
    }
    public override void ExitState(){}

}

public abstract class PlayerCameraState
{
    protected PlayerController PC;
    protected PlayerCameraState(PlayerController pc)
    {
        PC = pc;
    }
    public abstract void EnterState();
    public abstract void Tick();
    public abstract void ExitState();

}
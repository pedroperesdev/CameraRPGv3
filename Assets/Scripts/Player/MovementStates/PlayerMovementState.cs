using UnityEngine;

public abstract class PlayerMovementState
{
    protected PlayerController PC;

    protected PlayerMovementState(PlayerController pc)
    {
        PC = pc;
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    protected void MoveHorizontal(float speed, float wobbleFreq, float wobbleAmp)
    {
        Vector2 inputNorm = PC.MoveInput.normalized;
        Vector3 horizontal = PC.Cam.transform.forward * inputNorm.y * speed * Time.deltaTime + PC.Cam.transform.right * inputNorm.x * speed * Time.deltaTime;
        PC.ResultingMovement.x = horizontal.x;
        PC.ResultingMovement.z = horizontal.z;
        WobbleCamera(wobbleFreq, wobbleAmp);
    }

    protected void WobbleCamera(float wobbleFreq, float wobbleAmp)
    {
        PC.WobbleElapsed += Time.deltaTime;
        float offset = Mathf.Sin(PC.WobbleElapsed * wobbleFreq * 2 * Mathf.PI) * wobbleAmp;
        Vector3 newCamPos = new Vector3(PC.CameraGroupWobble.transform.localPosition.x, PC.StartingWobblePos + offset, PC.CameraGroupWobble.transform.localPosition.z);
        PC.CameraGroupWobble.transform.localPosition = newCamPos;
    }

    //Returns true while still moving back to the starting position
    protected bool StopWobble()
    {
        float newHeight = Mathf.Lerp(PC.CameraGroupWobble.transform.localPosition.y, PC.StartingWobblePos, 0.5f * Time.deltaTime);
        if(Mathf.Abs(newHeight - PC.StartingWobblePos) <= 0.01) {
            return false;
        }
        Vector3 newPos = PC.CameraGroupWobble.transform.localPosition;
        newPos.y = newHeight;
        PC.CameraGroupWobble.transform.localPosition = newPos;
        return true;
    }

}

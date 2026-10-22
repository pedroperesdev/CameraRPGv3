using UnityEngine;

public class AimCamState : PlayerCameraState
{
    private GameObject currEnemy = null;
    private float currRingVal = 0f;
    bool justTriedAimEnemy = false;

    public AimCamState(PlayerController pc) : base(pc) { }
    public override void EnterState()
    {
        PC.CurrAnimState = PlayerController.AnimationStates.StartAim;
        PC.SetCamHUD();
        PC.ClearHoverInfo();
    }
    public override void Tick()
    {
        if (!PC.AimAction.IsPressed())
        {
            PC.ChangeCameraState(PC.Lowered);
            return;
        }
        //logic like be able to take photo, show camera borders etc, actual camera snap behavior
        Transform camT = PC.Cam.transform;
        if(Physics.Raycast(PC.CameraObj.transform.position + camT.forward*PC.MinDistCamRay, camT.forward, out RaycastHit hit, PC.MaxDistCamRay)
            && hit.collider.CompareTag("Enemy"))
        {
            //If aiming at a different enemy
            if(currEnemy != hit.collider.gameObject)
            {
                TryClearCameraRing();
                currEnemy = hit.collider.gameObject;
                PC.InvokeEnemyAim(currEnemy);
            }
            justTriedAimEnemy = true;
            bool canSnap = currRingVal >= 1f;
            if(!canSnap)
            {
                currRingVal += Time.deltaTime/PC.CameraTimeUntilClick; //delta x in one frame
            }
            PC.InvokeCameraRingIncrease(currRingVal);
            if (PC.ShootAction.WasPressedThisFrame() && canSnap)
            {
                //On success this switches back to Lowered, which clears the ring on exit
                if (!PC.TryTakePhoto(hit.collider.GetComponent<Enemy>()))
                {
                    TryClearCameraRing();
                }
            }
        }
        else
        {
            TryClearCameraRing();
        }
    }
    public override void ExitState()
    {
        TryClearCameraRing();
    }

    //By using justTriedAimEnemy boolean, it only clears when needed instead of every frame
    void TryClearCameraRing()
    {
        if (justTriedAimEnemy)
        {
            currRingVal = 0f;
            PC.InvokeCameraRingIncrease(currRingVal);
            justTriedAimEnemy = false;
            currEnemy = null;
            PC.InvokeEnemyAim(null);
        }
    }

}

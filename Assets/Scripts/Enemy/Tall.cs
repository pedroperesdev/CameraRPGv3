using Unity.Mathematics;
using UnityEngine;

public class Tall : Enemy
{
    public override void CombatBehaviour()
    {
        Vector3 moveDir = (PlayerController.PlayerTransform.position - transform.position).normalized;
        moveDir.y = 0;
        Quaternion rot = Quaternion.LookRotation(moveDir);
        transform.rotation = rot;
        Vector3 walkDir = transform.forward * speed * Time.deltaTime;
        cc.Move(walkDir);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BotWeapon : Gun
{
    [HideInInspector]
    public Vector3 target;

    public override void Use()
    {
        Shoot(target);
    }

    void Shoot(Vector3 targetPos)
    {

    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        GameObject bulletObj = Instantiate(gunInfo.BulletPrefab, MuzzleObject.transform.position,
                                            MuzzleObject.transform.rotation);
    }
}

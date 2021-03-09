using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BotWeapon : Gun
{
    PhotonView PV;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    public override void Use(Vector3 targetPos)
    {
        Shoot(targetPos);
    }

    void Shoot(Vector3 targetPos)
    {
        PV.RPC("RPC_Shoot", RpcTarget.All, targetPos);
    }

    [PunRPC]
    void RPC_Shoot(Vector3 targetPos)
    {
        GameObject bulletObj = Instantiate(gunInfo.BulletPrefab, MuzzleObject.transform.position,
                                            MuzzleObject.transform.rotation);

        Destroy(bulletObj, 1.5f);
    }
}

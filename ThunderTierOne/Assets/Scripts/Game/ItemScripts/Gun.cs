using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Item
{

    public GunInfo gunInfo;
    public GameObject MuzzleObject;
    public GameObject bulletImpactPrefab;


    public override int CurrentBullet()
    {
        return gunInfo.currentBulletCount;
    }
    public override int CarryBulletCount()
    {
        return gunInfo.carryBulletCount;
    }

    public override void StartCurrnetBullet(int bullet)
    {
        gunInfo.currentBulletCount = bullet;
    }

    public override void Use()
    {

    }

    public virtual void Use(Vector3 target)
    {

    }

    public override void Reload()
    {
        
    }
    public override void DestroyBullet()
    {

    }
}

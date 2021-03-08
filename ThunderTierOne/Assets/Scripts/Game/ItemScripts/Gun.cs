﻿using System.Collections;
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

    public override void Use()
    {

    }

    public override void Reload()
    {
        
    }
    public override void DestroyBullet()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ThunderTierOne/New Gun")]
public class GunInfo : ItemInfo
{
    public float Damage;
    public int RecoilFactor;
    public float BulletSpeed;

    public GameObject BulletPrefab;
    public int Ammo;

    public int reloadBulletCount = 30;
    public int currentBulletCount = 30;
    public int carryBulletCount = 360;
}

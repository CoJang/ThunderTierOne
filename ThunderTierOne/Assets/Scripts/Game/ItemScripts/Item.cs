using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemInfo itemInfo;
    public GameObject itemGameObject;

    public abstract void StartCurrnetBullet(int bullet);
    public abstract int CarryBulletCount();
    public abstract int CurrentBullet();

    public abstract void Use();

    public abstract void Reload();

    public abstract void DestroyBullet();
}

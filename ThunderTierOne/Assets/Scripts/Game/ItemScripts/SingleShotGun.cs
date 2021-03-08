using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    PhotonView PV;

    [SerializeField] GameObject Bullet;
    List<GameObject> Bullets = null;
    GameObject obj;
    GameObject BulletParent;
    [SerializeField] int BulletIndex;
    public GameObject Muzzle;

    private void Awake()
    {
      
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        Bullets = new List<GameObject>();
        BulletIndex = 0;
        for (int i = 0; i < 30; i++)
        {
            obj = Instantiate(Bullet, Vector3.zero, Quaternion.Euler(Vector3.zero));
            BulletParent = GameObject.Find("BulletParent");
            obj.transform.parent = BulletParent.transform;
            obj.SetActive(false);
            Bullets.Add(obj);
          
        }
      
        gunInfo.carryBulletCount = 360;

        cam = GetComponentInParent<PlayerController>().playerCamera;
    }
    private void Update()
    {
        BulletParent.transform.position = new Vector3(Muzzle.transform.position.x, Muzzle.transform.position.y, Muzzle.transform.position.z);
    }

    public override void Use()
    {
        if(gunInfo.currentBulletCount > 0)
            Shoot();
    }

    public override void Reload()
    {
        if(gunInfo.currentBulletCount < 30)
        Reloading();
    }

    void Shoot()
    {   
        PV.RPC("RPC_Shoot", RpcTarget.All, null);
    }

    public override void DestroyBullet()
    {
        for (int i = 0; i < gunInfo.reloadBulletCount; ++i)
            Destroy(Bullets[i]);
    }

    [PunRPC]
    void RPC_Shoot()
    {
        Debug.Log("SHoot");

        while (Bullets[BulletIndex].activeInHierarchy)
        {
            BulletIndex = (BulletIndex + 1) % gunInfo.reloadBulletCount;
        }
        Bullets[BulletIndex].GetComponent<BulletPhysics>().Bulletdir = Muzzle.transform.forward;

        Bullets[BulletIndex].transform.position = Muzzle.transform.position;

        Bullets[BulletIndex].SetActive(true);
        
        gunInfo.currentBulletCount--;
    }



    void Reloading()
    {
        gunInfo.carryBulletCount += gunInfo.currentBulletCount;
        gunInfo.currentBulletCount = 0;


        if (gunInfo.carryBulletCount >= gunInfo.reloadBulletCount)
        {
            gunInfo.currentBulletCount = gunInfo.reloadBulletCount;
            gunInfo.carryBulletCount -= gunInfo.reloadBulletCount;
        }
        else
        {
            gunInfo.currentBulletCount = gunInfo.carryBulletCount;
            gunInfo.carryBulletCount = 0;
        }
    }

  
}

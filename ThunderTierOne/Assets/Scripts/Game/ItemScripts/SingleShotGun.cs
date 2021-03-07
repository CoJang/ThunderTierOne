using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    PhotonView PV;
    [SerializeField] int reloadBulletCount;
    [SerializeField] int currentBulletCount;
    [SerializeField] int carryBulletCount;

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
        for (int i = 0; i < reloadBulletCount; i++)
        {
            obj = Instantiate(Bullet, Vector3.zero, Quaternion.Euler(Vector3.zero));
            //BulletParent = GameObject.Find("BulletParent");
            //obj.transform.parent = BulletParent.transform;
            obj.SetActive(false);
            Bullets.Add(obj);
          
        }


        cam = GetComponentInParent<PlayerController>().playerCamera;
    }
    private void Update()
    {
        //BulletParent.transform.position = new Vector3(Muzzle.transform.position.x, Muzzle.transform.position.y, Muzzle.transform.position.z);
    }

    public override void Use()
    {
        if(currentBulletCount > 0)
            Shoot();
    }

    public override void Reload()
    {
        Reloading();
    }

    void Shoot()
    {   
        PV.RPC("RPC_Shoot", RpcTarget.All, null);
    }



    [PunRPC]
    void RPC_Shoot()
    {
        Debug.Log("SHoot");

        while (Bullets[BulletIndex].activeInHierarchy)
        {
            BulletIndex = (BulletIndex + 1) % reloadBulletCount;
        }
        Bullets[BulletIndex].GetComponent<BulletPhysics>().Bulletdir = Muzzle.transform.forward;

        Bullets[BulletIndex].transform.position = Muzzle.transform.position;

        Bullets[BulletIndex].SetActive(true);

        currentBulletCount--;
    }



    void Reloading()
    {
        carryBulletCount += currentBulletCount;
        currentBulletCount = 0;


        if (carryBulletCount >= reloadBulletCount)
        {
            currentBulletCount = reloadBulletCount;
            carryBulletCount -= reloadBulletCount;
        }
        else
        {
            currentBulletCount = carryBulletCount;
            carryBulletCount = 0;
        }
    }
}

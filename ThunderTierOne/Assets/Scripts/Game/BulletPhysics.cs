using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class BulletPhysics : MonoBehaviour
{
    public Transform StartTransform;
    
    public float BulletVelocity = 30.0f;
    [SerializeField] GameObject[] BulletImpact;

    Rigidbody rb;

    [SerializeField] Transform player;

    PlayerController pc;

    LineRenderer line;

    Vector3 dir;

    PhotonView PV;

    public Vector3 Bulletdir {  get { return dir; } set { dir = value; } }

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        pc = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody>();

        PV = GetComponent<PhotonView>();

    }



    public void DieDestroy()
    {
        Destroy(this);
    }

    IEnumerator ShootRay()
    {
        yield return new WaitForSeconds(2.0f);

        dir = Vector3.zero;
        this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        this.transform.position = Vector3.zero;
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {




        if (gameObject.activeSelf)
            StartCoroutine("ShootRay");



        // rb.AddForce(dir * BulletVelocity, ForceMode.Impulse);


        //        PV.RPC("PhysicBullet", RpcTarget.All, BulletVelocity);
        PhysicBullet(BulletVelocity);
    }


    public void PhysicBullet(float velocity)
    {
        this.transform.Translate(dir * velocity * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        switch (collision.transform.tag)
        {
            case "Metal":
                Instantiate(BulletImpact[0], transform.position, transform.rotation);
            
                break;
            case "Dirt":
                Instantiate(BulletImpact[1], transform.position, transform.rotation);
              
                break;
            case "Sand":
                Instantiate(BulletImpact[2], transform.position, transform.rotation);
               
                break;
            case "Wood":
                Instantiate(BulletImpact[3], transform.position, transform.rotation);
              
                break;
            case "Ground":
                Instantiate(BulletImpact[1], transform.position, transform.rotation);
            
                break;

            case "Player":
                Instantiate(BulletImpact[5], transform.position, transform.rotation);
                break;
            default:
                Instantiate(BulletImpact[2], transform.position, transform.rotation);
                break;
        }
        dir = Vector3.zero;
        this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        this.transform.position = Vector3.zero;
        this.gameObject.SetActive(false);
    }
}

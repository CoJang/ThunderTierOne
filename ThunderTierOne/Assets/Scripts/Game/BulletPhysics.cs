using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPhysics : MonoBehaviour
{
    public float BulletVelocity = 30.0f;
    [SerializeField] GameObject[] BulletImpact;

    Rigidbody rb;

    [SerializeField] Transform player;

    // Start is called before the first frame update
    void Start()
    {
      
        rb = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //float distance = Vector3.Distance(player.position, transform.position);

        //if (distance >= 200.0f)
        //{
        //    this.gameObject.SetActive(false);
        //    this.gameObject.transform.position = Vector3.zero;
        //}

        // rb.AddForce(bulletVec* BulletVelocity, ForceMode.Impulse);

        this.transform.Translate(new Vector3(0, 0, BulletVelocity * Time.deltaTime)); // 나중에 힘으로 바꿔야함.
    }

    void OnCollisionEnter(Collision collision)
    {
        switch (collision.transform.tag)
        {
            case "Metal":
                Instantiate(BulletImpact[0], transform.position, transform.rotation);
                this.gameObject.SetActive(false);
                break;
            case "Dirt":
                Instantiate(BulletImpact[1], transform.position, transform.rotation);
                this.gameObject.SetActive(false);
                break;
            case "Sand":
                Instantiate(BulletImpact[2], transform.position, transform.rotation);
                this.gameObject.SetActive(false);
                break;
            case "Wood":
                Instantiate(BulletImpact[3], transform.position, transform.rotation);
                this.gameObject.SetActive(false);
                break;
            case "Ground":
                Instantiate(BulletImpact[1], transform.position, transform.rotation);
                this.gameObject.SetActive(false);
                break;
            default:
                Instantiate(BulletImpact[2], transform.position, transform.rotation);
                this.gameObject.SetActive(false);
                break;
        }
    }
}

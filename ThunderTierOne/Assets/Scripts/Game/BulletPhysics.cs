﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPhysics : MonoBehaviour
{
    public float BulletVelocity = 30.0f;
    [SerializeField] GameObject[] BulletImpact;

    Rigidbody rb;

    [SerializeField] Transform player;

    PlayerController pc;

    LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        pc = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody>();


       
    }



    public void DieDestroy()
    {
        Destroy(this);
    }

    IEnumerator ShootRay()
    {
        yield return new WaitForSeconds(2.0f);

        Debug.DrawRay(transform.position, transform.forward * 50, Color.red);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
        {
            switch (hit.collider.tag)
            {
                case "Player":
                    Debug.Log("Player Hit");
                    break;

            }
        }
        else
        {
            Debug.Log("Null");
            rb.velocity = Vector3.zero;
           
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
      
        if (gameObject.activeSelf == false)
        {
            rb.velocity = Vector3.zero;
           

        }

   
        if (gameObject.activeSelf)
            StartCoroutine("ShootRay");

        Vector3 bulletVec = pc.Muzzle.transform.forward;
       
        rb.AddForce(bulletVec  * BulletVelocity, ForceMode.Impulse);

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
            default:
                Instantiate(BulletImpact[2], transform.position, transform.rotation);
                break;
        }
        rb.velocity = Vector3.zero;
        this.gameObject.SetActive(false);
    }
}

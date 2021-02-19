using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeEffect : MonoBehaviour
{
    public GameObject meshObj;
    public GameObject effectObj;
    public Rigidbody rigid;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Explosion");
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(1.5f);

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
      
        Instantiate(effectObj, this.transform.position,  Quaternion.identity);
        Destroy(this.gameObject, 0.5f);
       
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}

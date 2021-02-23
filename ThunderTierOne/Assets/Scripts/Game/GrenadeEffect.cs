using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeEffect : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] GameObject effectObj;
    [SerializeField] Rigidbody rigid;

    // Start is called before the first frame update
    void Start()
    {
        audioSource.Stop();
        StartCoroutine("Explosion");
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(1.5f);

        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        audioSource.Play();
        Instantiate(effectObj, this.transform.position,  Quaternion.identity);
        Destroy(this.gameObject, 1.0f);
       
    }
}

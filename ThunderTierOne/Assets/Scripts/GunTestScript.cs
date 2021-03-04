using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GunTestScript : MonoBehaviour
{
    public Transform MuzzleTransform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(MuzzleTransform.position, MuzzleTransform.forward * 10.0f, Color.green);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GunTestScript : MonoBehaviour
{
    public Transform MuzzleTransform;
    public PlayerController PC;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = MuzzleTransform.forward * 10.0f;
        Debug.DrawLine(MuzzleTransform.position, MuzzleTransform.position + forward, Color.green);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Camera cam;

    public void BindCamera(Camera camera)
    {
        cam = camera;
    }

    void Update()
    {
        //if(cam != null)
        //{
        //    transform.LookAt(cam.transform.forward);
        //}
        //else
        {
            transform.rotation = Quaternion.identity;
        }
    }
}

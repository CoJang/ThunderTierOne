using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class CursorChange : MonoBehaviour
{
   
    Camera playerCamera;


    // Start is called before the first frame update
    void Start()
    {
       //playerCamera = GameObject.Find("Camera").GetComponent<Camera>();



    }

    public void FindCamera(Camera camera)
    {
        playerCamera = camera;
   
    }
    
   

    // Update is called once per frame
    void Update()
    {   
        //playerCamera = GameObject.Find("Camera").GetComponent<Camera>();

        
        //if(playerCamera != null)
        //      LookAt();
    }

    void OnMouseEnter()
    {
       // PlayerAim.Instance.TargetCursor();
    }

    void OnMouseExit()
    {
        //PlayerAim.Instance.DefaultCursor();
    }

    void LookAt()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        ray.origin = playerCamera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.tag == "Player")
            {
                PlayerAim.Instance.TargetCursor();
                Debug.Log("Player");
            }
        
        else
        {
            PlayerAim.Instance.DefaultCursor();
        }
        }
       



    }
}

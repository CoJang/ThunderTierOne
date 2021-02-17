using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ReloadCursor : MonoBehaviour
{
   
  
    [SerializeField]
    float rotateSpeed = 80.0f;

    private SpriteRenderer rend;
    public GameObject ReloadImage;


    // Start is called before the first frame update
    void Start()
    {

        Cursor.visible = false;
        rend = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

       
        //Vector3 mousePosition = playerCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        //transform.rotation = Quaternion.Euler(new Vector3(0, 60, 0));
        transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));



        transform.position = Input.mousePosition;

    }

 


    public void Reload()
    {
        ReloadImage.SetActive(true);
       
        
    }
  
    public void ReloadEnd()
    {
        ReloadImage.SetActive(false);

    }


}

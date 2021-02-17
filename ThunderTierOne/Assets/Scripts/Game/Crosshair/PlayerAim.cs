using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerAim : MonoBehaviour
{
    //public Texture2D cursorTexture;
    //public Texture2D cursorTexture2;

    private SpriteRenderer rend;
     [SerializeField]
       Image  cursorSprite;
    

    public static PlayerAim Instance;

    Camera playerCamera;

    private Vector2 hotSpot;
    
    bool targetOn = false;



    public void Awake()
    {

        rend = GetComponent<SpriteRenderer>();
        //ReloadImage = Resources.Load<Sprite>("Reload");
        Instance = this;
        
        //Cursor.lockState = CursorLockMode.Confined;
    }

    void Start()
    {
       
    }

   void Update()
    {
        transform.position = Input.mousePosition;
    }

 

    void StartCursor(Texture2D cursorType)
    {   
        //hotSpot.x = cursorTexture.width / 2;
        //hotSpot.y = cursorTexture.height / 2;
        //Cursor.SetCursor(cursorType, hotSpot, CursorMode.Auto);
    }

    public void DefaultCursor()
    {
        cursorSprite.color = new Color(1,1,1);

    }
    public void TargetCursor()
    {
        cursorSprite.color = new Color(1,0,0);
    }

   



}

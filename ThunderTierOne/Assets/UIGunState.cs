using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIGunState : MonoBehaviour
{

    [SerializeField] Image[] image;

    PlayerController pc;
            
    Color color;


   public void GetPlayerController(GameObject Gpc)
    {
        pc = Gpc.GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Awake()
    {

        

    }


    // Update is called once per frame
    void Update()
    {

        
        for(int i = 0; i < image.Length; ++i)
        {
            if (i == pc.ItemIndex)
            {
                color = new Color(1, 0.5f, 0);
                image[i].color = color;

            }
            else
            {
                color = new Color(1, 1, 1);
                image[i].color = color;
            }

        }
         
        
       
    }
}

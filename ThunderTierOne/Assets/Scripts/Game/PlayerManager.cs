using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameObject topdownCamera;
    [SerializeField] GameObject InteractHUD;
    [SerializeField] GameObject CursorChange;
    //[SerializeField] GameObject playerIndicator;

    PhotonView PV;
    GameObject controller;
    GameObject playerCamera;
    GameObject interactHUD;



    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        if(PV.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();

        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NoCamPlayerController"),
                        spawnPoint.position, spawnPoint.rotation, 0, new object[] { PV.ViewID });

        
        controller.tag = "MyChar";

        playerCamera = Instantiate(topdownCamera, spawnPoint.position, Quaternion.Euler(60, 0, 0));
        Camera camera = playerCamera.GetComponentInChildren<Camera>();
        //indicator = Instantiate(playerIndicator, spawnPoint.position, Quaternion.identity);

        //indicator.GetComponent<CameraFollow>().SetTarget(controller.transform);
        playerCamera.GetComponent<CameraFollow>().SetTarget(controller.transform);
        controller.GetComponent<PlayerController>().BindPlayerCamera(camera);

        controller.GetComponentInChildren<BillBoard>().BindCamera(camera);
        controller.GetComponentInChildren<Image>().color = new Color32(255, 151, 26, 255);

        CursorChange.GetComponent<CursorChange>().FindCamera(playerCamera.GetComponent<Camera>()) ;

        GameObject Canvas = GameObject.Find("Canvas");
        interactHUD = Instantiate(InteractHUD);
        interactHUD.transform.SetParent(Canvas.transform);
        RectTransform HUDTransform = interactHUD.GetComponent<RectTransform>();
        HUDTransform.anchoredPosition = Vector2.zero;
        HUDTransform.localScale = Vector3.one;

        controller.GetComponent<PlayerController>().BindHUD(interactHUD);
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        Destroy(playerCamera);
        Destroy(interactHUD);
        CreateController();
    }
}

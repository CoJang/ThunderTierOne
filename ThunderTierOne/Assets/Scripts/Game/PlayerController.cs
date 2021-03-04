using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable, IPunObservable, IInteractable
{
    #region Animator Variables
    public Animator anim;
    Vector2 AnimControlVelocity = Vector2.zero;
    float CrouchingDecreaseFactor = 0.1f;
    float DecreaseFactor = 0.1f;
    float MaxAnimVelocity = 1.0f;

    bool IsSwapDelay = false;
    bool IsReloading = false;
    bool Crouching = false;
    bool IsThrowing = false;
    bool Aiming = false;
    bool isShooting = false;
    bool isobscuration = false;
    bool isCovering = false;
    bool Cover = false;
    bool isRunning = false;
    #endregion

    int IsReloadingHash = Animator.StringToHash("IsReloading");
    //string  PlayerHUDText = "HOLD  F        상호작용";
    //Vector2 PlayerHUDPivot = new Vector2(0.306155f, 0.325f);

    #region Item Variables
    [SerializeField] Item[] items;
    int itemIndex = 0;
    int preItemIndex = -1;

    public int ItemIndex { get { return itemIndex; } set { itemIndex = value; } }
    public GameObject Getmuzzle { get { return Muzzle; } }
    #endregion

    #region Charactor Movement Variables
    bool isGrounded;
    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;
    [SerializeField] float silencespeed, walkSpeed, crouchingSpeed, smoothTime, jumpForce , runSpeed;
    #endregion

    #region Player Control Essentials
    [HideInInspector] public Camera playerCamera;
    Rigidbody rb;
    PhotonView PV;
    PlayerManager playerManager;
    Indicator indicator;
    #endregion

    #region Spine Rotation Variables
    Transform spine;
    float MaxYAxis = 2.5f;
    Vector3 relativeVec = new Vector3(0, -55, -100);
    public Vector3 lookTarget = Vector3.zero;
    #endregion

    #region GamePlay Variables
    [SerializeField] const float maxHealth = 100.0f;
    [SerializeField] float currentHealth = maxHealth;
    #endregion

    ReloadCursor ReloadImage;

    //---------------Grenade 
    [SerializeField] float throwVelocity;
    [SerializeField] Transform throwPoint;
    [SerializeField] GameObject grenade, GrenadeOrbit;

    //--------------Gun
    [SerializeField] GameObject Bullet;
    List<GameObject> Bullets = null;
    GameObject obj;
    [SerializeField] int BulletIndex;
    Transform muzzleOriginTransform;
    public GameObject Muzzle;
    [SerializeField] GameObject BulletEffect;

    //Delay
    private float fireRate = 0.1f; //총알 지연 시간 설정
    private float nextFire = 0.0f; //다음 총알 발사시간

    //Bullet
    [SerializeField] float G_Count = 2;

    [SerializeField] int reloadBulletCount;
    [SerializeField] int currentBulletCount;
    [SerializeField] int carryBulletCount;

    // Temp Player State
    [SerializeField] bool isDowned = false;
    [SerializeField] Transform HandleTransform;
    [SerializeField] Transform GunTransform;
    bool isInteractable = false;
    GameObject InteractHUD;
    Reticle reticle;


    public bool CrouchingState { get { return Crouching; } set { Crouching = value; } }
    private void Awake()
    {
        ReloadImage = GameObject.Find("Reload").GetComponent<ReloadCursor>();
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);
      
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

        muzzleOriginTransform = Muzzle.transform;
    }

    private void Start()
    {
        reticle = GameObject.Find("Reticle").GetComponent<Reticle>();
        reticle.SetActive(false);
        indicator = GetComponentInChildren<Indicator>();

        Bullets = new List<GameObject>();
        BulletIndex = 0;
        for (int i = 0; i < 15; i++)
        {
            obj = Instantiate(Bullet,Vector3.zero, Quaternion.Euler(Vector3.zero));
            obj.transform.parent = GameObject.Find("BulletParent").transform;
            obj.SetActive(false);
            Bullets.Add(obj);
           
        }
        
        if (PV.IsMine)
        {
            EquipItem(0);
        }
        else
        {
            //Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }

    //반동
   public Vector3 RandReCoil;
    public int minRecoil = 78;
    public int maxRecoil = 83;

    [PunRPC]
    void GunFiring()
    {
        Bullets[BulletIndex].transform.position = Muzzle.transform.position;
        Bullets[BulletIndex].transform.rotation = Muzzle.transform.rotation;


        RandReCoil.x = Random.Range(minRecoil, maxRecoil);

       Muzzle.transform.localRotation = Quaternion.Euler(RandReCoil.x, 0 , 0);


        while (Bullets[BulletIndex].activeInHierarchy)
        {
            BulletIndex = (BulletIndex+1) % reloadBulletCount;
        }
       
        Bullets[BulletIndex].SetActive(true);
        BulletEffect.SetActive(true);
        currentBulletCount--;
      

        //rigidBullet.AddTorque(Vector3.back * 5, ForceMode.Impulse);// 회전
    }

    public void OnCollisionEnter(Collision collision)
    {
        switch (collision.transform.tag)
        {
            case "Bullet":
                Debug.Log("TakeDamage");

                Bullets[BulletIndex].transform.position = new Vector3(-99.0f, -99.0f, -99.0f);
                Bullets[BulletIndex].transform.rotation = Quaternion.identity;
                TakeDamage(25);
                break;

           
        }   
    }

    void OnTriggerEnter(Collider other)
    {
        if(!isDowned && other.tag == "Player")
        {
            RectTransform CanvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
            Vector2 ViewportPosition = playerCamera.WorldToViewportPoint(other.transform.position);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            (ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f),
            (ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f));

            isInteractable = true;
            InteractHUD.SetActive(true);
            InteractHUD.GetComponent<RectTransform>().anchoredPosition = WorldObject_ScreenPosition;
            Debug.Log("상호작용 가능한 플레이어가 근처에 있습니다.");
            Interact(other);
        }

        if (other.tag == "Wall")
        {
            InteractHUD.SetActive(true);
            InteractHUD.GetComponent<RectTransform>().anchoredPosition = transform.position;
            isCovering = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isDowned && other.tag == "Player")
        {
            RectTransform CanvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
            Vector2 ViewportPosition = playerCamera.WorldToViewportPoint(other.transform.position);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            (ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f),
            (ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f));

            InteractHUD.GetComponent<RectTransform>().anchoredPosition = WorldObject_ScreenPosition;
            Interact(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case "Wall":
                InteractHUD.SetActive(false);
                isCovering = false;
                break;
            case "Player":
                InteractHUD.SetActive(false);
                isInteractable = false;
                break;
        }
    }

    private void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }
    
        Move();
        Jump();
        SwapWeapon();
     
        if(currentBulletCount > 0)
            Shoot();

        Reload();
        Covering();
     
        if(!isDowned)
            anim.SetBool("KnockDown", false);

        if (transform.position.y < -10f)
        {
            Die();
        }

        if (itemIndex == 2)
            GrednadeThrow();
        else
            GrenadeOrbit.SetActive(false);
    }
    
    void Covering()
    {
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y +0.5f, transform.position.z), transform.forward * 50, Color.red);

        //if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), transform.forward, out RaycastHit hit))
        //{
        //    switch (hit.collider.tag)
        //    {
        //        case "Wall":
        //            isCovering = true;
        //            break;

        //    }
        //}
        //else
        //    isCovering = false;

        if (isCovering)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                anim.SetBool("Cover", true);

                Cover = true;
                ChangeIndicator(Indicator.INDICATOR.COVERED);
            }
           
        }
        else
        {
            Cover = false;
            anim.SetBool("Cover", false);

            if(!isDowned)
                ChangeIndicator(Indicator.INDICATOR.NORMAL);
        }

        if (anim.GetCurrentAnimatorStateInfo(3).IsName("Cover"))
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                anim.SetBool("Cover", false);
                Cover = false;

                if (!isDowned)
                    ChangeIndicator(Indicator.INDICATOR.NORMAL);
            }
        }

    }

    void Reload()
    {
        
        if (currentBulletCount == 0 && carryBulletCount == 0)
        {
            photonView.RPC("OffEffect", RpcTarget.All, null);
            return;
        }
        if (Input.GetKeyDown(KeyCode.R) && !anim.GetBool(IsReloadingHash) && currentBulletCount < reloadBulletCount)
        {
            photonView.RPC("OffEffect", RpcTarget.All, null);
            photonView.RPC("AniReload", RpcTarget.All, null);
        }
        else
        {
            anim.SetBool(IsReloadingHash, false);
        }

        if (currentBulletCount == 0)
        {
            photonView.RPC("OffEffect", RpcTarget.All, null);
            anim.SetBool(IsReloadingHash, true);

        }        
    }


    [PunRPC]
    void AniReload()
    {
        if(PV.IsMine)
        anim.SetBool(IsReloadingHash, true);
    }

    void Shoot()
    {
        if (IsReloading || IsSwapDelay)
            return;

        if (itemIndex < 2)
        {
            bool isLeftDown = Input.GetMouseButton(0);
            bool isRightDown = Input.GetMouseButton(1);

            if (isRightDown)
            {
                anim.SetBool("Aiming", true);
                reticle.SetActive(true);
            }

            if (isLeftDown)
            {
                if (Aiming && Time.time > nextFire)
                {
                    nextFire = Time.time + fireRate;

                    SoundManager.Instance.Fire();
                    anim.SetBool("Firing", true); //반동 애니메이션
                    reticle.SetReticleSize(reticle.reticleSize + (RandReCoil.x - minRecoil) * 4);
                    photonView.RPC("GunFiring", RpcTarget.All, null);
                    //items[itemIndex].Use();
                    reticle.SetActive(true);
                }
                else
                {
                    reticle.SetActive(true);
                    anim.SetBool("Aiming", true);
                    anim.SetBool("Firing", false);
                }
            }
            else
            {
                photonView.RPC("OffEffect", RpcTarget.All, null);
                anim.SetBool("Firing", false);
                Muzzle.transform.localRotation = muzzleOriginTransform.localRotation;
            }

            if (!isLeftDown && !isRightDown)
            {
                Aiming = false;
                anim.SetBool("Aiming", false);
                reticle.SetActive(false);
            }

        }
    }

    private void LateUpdate()
    {
        //if(!isDowned)
            Look();

        if (moveAmount.normalized.magnitude.AlmostEquals(1.0f, 0.1f))
        {
            reticle.SetReticleSize(reticle.reticleSize + moveAmount.magnitude);
        }
        else
        {
            float destSize = Mathf.Lerp(reticle.reticleSize, 120, Time.deltaTime);
            reticle.SetReticleSize(destSize);
        }
    }

    [PunRPC]
    void OffEffect()
    {
        BulletEffect.SetActive(false);
    }

    void SwapWeapon()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);

                break;
            }
        }


        //if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        //{
        //    if (itemIndex >= items.Length - 1)
        //    {
        //        EquipItem(0);
        //    }
        //    else
        //    {
        //        EquipItem(itemIndex + 1);
        //    }
        //}
        //else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        //{
        //    if (itemIndex <= items.Length - 1)
        //    {
        //        EquipItem(items.Length - 1);
        //    }
        //    else
        //    {
        //        EquipItem(itemIndex - 1);
        //    }
        //}
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

      

        if (Input.GetKey(KeyCode.C) )
        {
            Crouching = true;
            anim.SetBool("Crouching", true);
            moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * crouchingSpeed,
                        ref smoothMoveVelocity, smoothTime);
           
        }
        else
        {
            Crouching = false;
            anim.SetBool("Crouching", false);
            moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftControl) ? silencespeed : walkSpeed),
                        ref smoothMoveVelocity, smoothTime);
        }

        if (!Cover&& !Crouching && Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
            anim.SetBool("Running", true);
            isRunning = true;
            moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * runSpeed,
                       ref smoothMoveVelocity, smoothTime);
        }
        else
        {
            isRunning = false;
            anim.SetBool("Running", false);
        }

        if (Cover)
            moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * crouchingSpeed,
                       ref smoothMoveVelocity, smoothTime);

        AnimControlVelocity.x += moveAmount.x * Time.deltaTime * 2;
        AnimControlVelocity.y += moveAmount.z * Time.deltaTime;

        AnimControlVelocity.x = Mathf.Clamp(AnimControlVelocity.x, -MaxAnimVelocity, MaxAnimVelocity);
        AnimControlVelocity.y = Mathf.Clamp(AnimControlVelocity.y, -MaxAnimVelocity, MaxAnimVelocity);

        if (moveDir.x == 0 && AnimControlVelocity.x > 0)
        {
            if (Crouching || Cover)
                AnimControlVelocity.x -= CrouchingDecreaseFactor;
            else
                AnimControlVelocity.x -= DecreaseFactor;

            if (AnimControlVelocity.x < 0)
                AnimControlVelocity.x = 0;
        }
        else if (moveDir.x == 0 && AnimControlVelocity.x < 0)
        {


            if (Crouching || Cover)
                AnimControlVelocity.x += CrouchingDecreaseFactor;
            else
                AnimControlVelocity.x += DecreaseFactor;

            if (AnimControlVelocity.x > 0)
                AnimControlVelocity.x = 0;
        }

        if (moveDir.z == 0 && AnimControlVelocity.y > 0)
        {
            if (Crouching)
                AnimControlVelocity.y -= CrouchingDecreaseFactor;
            else
                AnimControlVelocity.y -= DecreaseFactor;

            if (AnimControlVelocity.y < 0)
                AnimControlVelocity.y = 0;
        }
        else if (moveDir.z == 0 && AnimControlVelocity.y < 0)
        {
            if (Crouching)
                AnimControlVelocity.y += CrouchingDecreaseFactor;
            else
                AnimControlVelocity.y += DecreaseFactor;

            if (AnimControlVelocity.y > 0)
                AnimControlVelocity.y = 0;
        }

        anim.SetFloat("Horizontal", AnimControlVelocity.x);
        anim.SetFloat("Vertical", AnimControlVelocity.y);

       

        if (Crouching)
        {
            anim.SetFloat("Horizontal", AnimControlVelocity.x * 0.8f);
            anim.SetFloat("Vertical", AnimControlVelocity.y * 0.8f);
        }

        if (isCovering)
        {
            anim.SetFloat("Horizontal", AnimControlVelocity.x );
            anim.SetFloat("Vertical", AnimControlVelocity.y );
        }
    }

    void Look()
    {
        if (!PV.IsMine)
        {
            //GunTransform.LookAt(lookTarget);

            //spine.LookAt(lookTarget);
            //Quaternion spineRot = spine.rotation * Quaternion.Euler(relativeVec);
            //spine.rotation = spineRot;
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        ray.origin = playerCamera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.tag == "MyChar")
                return;

            //GunTransform.rotation.
            lookTarget = hit.point;
            lookTarget.y = Mathf.Clamp(lookTarget.y, 0, MaxYAxis);
            transform.LookAt(new Vector3(lookTarget.x, 0, lookTarget.z));

            spine.LookAt(lookTarget);
            Quaternion spineRot = spine.rotation * Quaternion.Euler(relativeVec);
            spine.rotation = spineRot;

            if(hit.collider.tag == "Player")
                PlayerAim.Instance.TargetCursor();
            else
                PlayerAim.Instance.DefaultCursor();

        }

        //if (Physics.Raycast(BulletPos.transform.position, BulletPos.transform.forward, out hit, 30))
        //{
        //    Debug.Log(hit.collider.gameObject.name);
        //    Debug.DrawLine(lookTarget, hit.point, Color.green);
        //}

    }
    void GrednadeThrow()
    {
        if (G_Count > 0)
        {
            if (Input.GetMouseButton(0) && !anim.GetBool("Throw") && !IsSwapDelay)
            {
                anim.SetBool("ThrowIdle", true);
            }
            else
            {
                anim.SetBool("ThrowIdle", false);
            }
      
            if (Input.GetMouseButtonUp(0) && !anim.GetBool("Throw"))
            {
                GrenadeOrbit.SetActive(false);
                anim.SetBool("Throw", true);
                StartCoroutine("DelayThrow");

            }
        }

        if(G_Count == 0)
        {
            EquipItem(0);
        }
    }
    IEnumerator DelayThrow()
    {
        yield return new WaitForSeconds(1.5f);
        anim.SetBool("Throw", false);
    }

    void Grenade()
    {
        Vector3 nextVec = throwPoint.transform.forward * throwVelocity;
        nextVec.y = 5.0f;
        GameObject instanceGrenade = Instantiate(grenade, throwPoint.transform.position, throwPoint.transform.rotation);
        Rigidbody rigidGrenade = instanceGrenade.GetComponent<Rigidbody>();
        rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
        rigidGrenade.AddTorque(Vector3.back * 5, ForceMode.Impulse);// 회전
    }

    public void SetGroundedState(bool grounded)
    {
        isGrounded = grounded;
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
        {
            return;
        }

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    void EquipItem(int _index)
    {
        if (_index == preItemIndex)
            return;

        if (G_Count == 0 && _index == 2)
            return;

        itemIndex = _index;
        Aiming = false;

        switch (itemIndex)
        {
            case 0: // Rifle
            case 1: // Pistol         
                anim.SetTrigger("Swap");
                anim.SetBool("ThrowIdle", false);
                break;
            case 2: // Grenade
                anim.SetBool("Aiming", false);
                anim.SetTrigger("SwapGrenade");
                break;
            case 3://healthpack
                anim.SetTrigger("Swap");

                break;
        }

        items[itemIndex].itemGameObject.SetActive(false);
        anim.SetLayerWeight(itemIndex, 1);

   

        if (preItemIndex != -1)
        {
            items[preItemIndex].itemGameObject.SetActive(false);
            anim.SetLayerWeight(preItemIndex, 0);
        }

        preItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    // 총을 쏜 사람의 컴퓨터에서 실행된다.
    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    // 모두의 컴퓨터에서 실행된다.
    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            if(!isDowned)
                KnockDown();
            else
            {
                for(int i =0; i <15; ++i)
                Destroy(Bullets[i]);
                Die();
            }
        }
    }

    void Die()
    {
   
        isDowned = false;
        playerManager.Die();
    }

    public void BindPlayerCamera(Camera camera)
    {
        playerCamera = camera;
    }

    public void BindHUD(GameObject HUD)
    {
        InteractHUD = HUD;
        InteractHUD.SetActive(false);
    }


    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(lookTarget);
        }
        else
        {
            lookTarget = (Vector3)stream.ReceiveNext();
        }
    }

    public Player GetPhotonViewOwner()
    {
        return PV.Owner;
    }
    // 애니메이션 이벤트
    void OnThrowStart()
    {
        //var clone = Instantiate(grenade);
        //this.simul.Shoot(clone, startPoint.position, endPoint.position, g, heightGo.position.y);
        Debug.Log("Throw");
        if (!IsSwapDelay)
        {
            G_Count -= 1;
            Grenade();
        }
        GrenadeOrbit.SetActive(false);

        IsThrowing = true;
    }

    void OnThrowEnd()
    {
        if (PV.IsMine)
        {
            if (itemIndex == 2)
                GrenadeOrbit.SetActive(true);
        }
        IsThrowing = false;
    }
    public void OnSwapStart()
    {
        IsSwapDelay = true;
        ReloadImage.ReloadEnd();
        Debug.Log("SwapStart");
    }
    public void OnSwapFinish()
    {
        if (IsReloading == true)
            IsReloading = false;

        items[itemIndex].itemGameObject.SetActive(true);
        StartCoroutine("ItemDelay");

        Debug.Log("SwapEnd");
        IsSwapDelay = false;
    }

    IEnumerator ItemDelay()
    {
        yield return new WaitForSeconds(1.5f);
    }


    public void OnReloadingStart()
    {
        ReloadImage.Reload();
        SoundManager.Instance.Reload();
      
        IsReloading = true;
    }

    void OnReloadingEnd()
    {
        ReloadImage.ReloadEnd();
     
        IsReloading = false;

        carryBulletCount += currentBulletCount;
        currentBulletCount = 0;

        if (carryBulletCount >=reloadBulletCount)
        {
            currentBulletCount = reloadBulletCount;
            carryBulletCount -= reloadBulletCount;
        }
        else
        {
            currentBulletCount = carryBulletCount;
            carryBulletCount = 0;
        }
    }

    void OnShootingStart()
    {
        Aiming = true;
    }

    void KnockDown()
    {
        isDowned = true;
        currentHealth = 55;
        Debug.Log("I'm Down!");
        PV.RPC("RPC_PlayerDown", RpcTarget.All, true);
        anim.SetBool("KnockDown", true);
        ChangeIndicator(Indicator.INDICATOR.DOWNED);
    }

    [PunRPC]
    void RPC_PlayerDown(bool isdowned)
    {
        if (PV.IsMine)
            return;

        if (isdowned)
        {
            Debug.Log("Player Down! " + isdowned);
        }
        else
        {
            Debug.Log("Player Recovered! " + isdowned);
        }

        isDowned = isdowned;
        GetComponent<SphereCollider>().enabled = isdowned;

        currentHealth = 55;
    }

    float pressedTime = 0.0f;
    void Interact(Collider other)
    {
        if (!isInteractable)
            return;

        if (Input.GetKey(KeyCode.F))
        {
            pressedTime += Time.deltaTime;
            anim.SetBool("Save", true);
            if (pressedTime >= 2.0f)
            {
                anim.SetBool("Save", false);
            
                pressedTime = 0;
                other.GetComponent<IInteractable>()?.Interaction();
                InteractHUD.SetActive(false);
            }
        }
        else
        {
            anim.SetBool("Save", false);
        }
    }

    public void Interaction()
    {
        if (isDowned)
        {
            PV.RPC("Save", RpcTarget.All);
            currentHealth = 55;
            PV.RPC("RPC_PlayerDown", RpcTarget.All, false);

            ChangeIndicator(Indicator.INDICATOR.NORMAL);
            Debug.Log("Player Recovered!");
        }
    }
    
    [PunRPC]
    void Save()
    {
        isDowned = false;
    }


    void ReticleEffect()
    {
        reticle.SetReticleSize(reticle.reticleSize + moveAmount.magnitude);
    }


    void WalkSound()
    {
        if (anim.GetCurrentAnimatorStateInfo(3).IsName("Walk"))
            SoundManager.Instance.Walk();

        if (anim.GetCurrentAnimatorStateInfo(3).IsName("Crouching"))
            SoundManager.Instance.Crouching();

        if (anim.GetCurrentAnimatorStateInfo(3).IsName("Run"))
            SoundManager.Instance.Walk();
    }
    void ChangeIndicator(Indicator.INDICATOR indicatorType)
    {
        // Play on Local
        indicator.ChangeIndicator(indicatorType);

        // Play on Remote
        PV.RPC("RPC_SyncIndicator", RpcTarget.Others, indicatorType);
    }

    [PunRPC]
    void RPC_SyncIndicator(Indicator.INDICATOR indicatorType)
    {
        indicator.ChangeIndicator(indicatorType);
    }

    // 애니메이터의 IK 갱신
    private void OnAnimatorIK(int layerIndex)
    {
        if (IsReloading || IsSwapDelay)
            return;

        // IK를 사용하여 왼손의 위치와 회전을 총의 오른쪽 손잡이에 맞춘다
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        anim.SetIKPosition(AvatarIKGoal.LeftHand, HandleTransform.position);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, HandleTransform.rotation);

    }
}
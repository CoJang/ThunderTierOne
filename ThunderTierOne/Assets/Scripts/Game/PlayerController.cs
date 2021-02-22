using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable, IPunObservable
{
    #region Animator Variables
    Animator anim;
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
    #endregion

    #region Item Variables
    [SerializeField] Item[] items;
    int itemIndex = 0;
    int preItemIndex = -1;
    #endregion

    #region Charactor Movement Variables
    bool isGrounded;
    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;
    [SerializeField] float silencespeed, walkSpeed, crouchingSpeed, smoothTime, jumpForce;
    #endregion

    #region Player Control Essentials
    [HideInInspector] public Camera playerCamera;
    Rigidbody rb;
    PhotonView PV;
    PlayerManager playerManager;
    #endregion

    #region Spine Rotation Variables
    Transform spine;
    float MaxYAxis = 2.5f;
    Vector3 relativeVec = new Vector3(0, -55, -100);
    Vector3 lookTarget = Vector3.zero;
    #endregion

    #region GamePlay Variables
    const float maxHealth = 100.0f;
    float currentHealth = maxHealth;
    #endregion

    ReloadCursor ReloadImage;

    //---------------Grenade 
    [SerializeField] float throwVelocity;
    [SerializeField] Transform throwPoint;
    [SerializeField] GameObject grenade, GrenadeOrbit;

    //--------------Gun
    [SerializeField] float BulletVelocity;
    [SerializeField] GameObject Bullet;
    [SerializeField] GameObject Muzzle;
    [SerializeField] GameObject BulletEffect;

    //Delay
    private float fireRate = 0.1f; //총알 지연 시간 설정
    private float nextFire = 0.0f; //다음 총알 발사시간

    //



    private void Awake()
    {
        ReloadImage = GameObject.Find("Reload").GetComponent<ReloadCursor>();
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start()
    {
       

        if (PV.IsMine)
        {
            EquipItem(0);
        }
        else
        {
            //Destroy(GetComponentInChildren<Camera>().gameObject);
            //Destroy(rb);
        }
    }

    //반동
    Vector3 RandReCoil;
    [PunRPC]
    void GunFiring()
    {
        Vector3 nextVec = Muzzle.transform.forward * BulletVelocity;

        GameObject instanceBullet =Instantiate(Bullet, Muzzle.transform.position,
            Muzzle.transform.rotation);

        Rigidbody rigidBullet = instanceBullet.GetComponent<Rigidbody>();
        rigidBullet.AddForce(nextVec, ForceMode.Impulse);
     
        RandReCoil.x = Random.Range(75, 85);

        Muzzle.transform.localRotation = Quaternion.Euler(RandReCoil.x, 330, 0);
        Destroy(instanceBullet, 1.5f);
        //rigidBullet.AddTorque(Vector3.back * 5, ForceMode.Impulse);// 회전
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
        Shoot();
        
        if (Input.GetKeyDown(KeyCode.R) && !anim.GetBool("IsReloading"))
        {
            Instantiate(Bullet, transform.position, transform.rotation);
            anim.SetBool("IsReloading", true);
        }
        else
        {
            anim.SetBool("IsReloading", false);
        }

        if(transform.position.y < - 10f)
        {
            Die();
        }

        if (itemIndex == 2)
            GrednadeThrow(); // 인풋

        if (itemIndex == 1 || itemIndex == 0)
        {
            GrenadeOrbit.SetActive(false);
        }


     


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
            }

            if (isLeftDown)
            {
                if (Aiming && Time.time > nextFire)
                {
                    nextFire = Time.time + fireRate;

             
                    anim.SetBool("Firing", true); //반동 애니메이션
                    photonView.RPC("GunFiring", RpcTarget.All, null);
                }
                else
                {
                    anim.SetBool("Aiming", true);
                    anim.SetBool("Firing", false);
                }
            }
            else
            {
                anim.SetBool("Firing", false);
            }

            if(!isLeftDown && !isRightDown)
            {
                Aiming = false;
                anim.SetBool("Aiming", false);
            }

            if(!isLeftDown)
                Muzzle.transform.localRotation = Quaternion.Euler(80, 330, 0); 
        }
    }

    private void LateUpdate()
    {
        Look();
        


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

        if (Input.GetKey(KeyCode.LeftControl))
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
            moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? silencespeed : walkSpeed),
                        ref smoothMoveVelocity, smoothTime);
        }


        AnimControlVelocity.x += moveAmount.x * Time.deltaTime * 2;
        AnimControlVelocity.y += moveAmount.z * Time.deltaTime;

        AnimControlVelocity.x = Mathf.Clamp(AnimControlVelocity.x, -MaxAnimVelocity, MaxAnimVelocity);
        AnimControlVelocity.y = Mathf.Clamp(AnimControlVelocity.y, -MaxAnimVelocity, MaxAnimVelocity);

        if (moveDir.x == 0 && AnimControlVelocity.x > 0)
        {
            if (Crouching)
                AnimControlVelocity.x -= CrouchingDecreaseFactor;
            else
                AnimControlVelocity.x -= DecreaseFactor;

            if (AnimControlVelocity.x < 0)
                AnimControlVelocity.x = 0;
        }
        else if (moveDir.x == 0 && AnimControlVelocity.x < 0)
        {


            if (Crouching)
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

        if(Crouching)
        {
            anim.SetFloat("Horizontal", AnimControlVelocity.x * 0.8f);
            anim.SetFloat("Vertical", AnimControlVelocity.y * 0.8f);
        }
    }

    void Look()
    {
        if (!PV.IsMine)
        {
            spine.LookAt(lookTarget);
            Quaternion spineRot = spine.rotation * Quaternion.Euler(relativeVec);
            spine.rotation = spineRot;
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        ray.origin = playerCamera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider.tag == "MyChar") 
                return;

            lookTarget = hit.point;
            lookTarget.y = Mathf.Clamp(lookTarget.y, 0, MaxYAxis);
            spine.LookAt(lookTarget);
            transform.LookAt(new Vector3(lookTarget.x, 0, lookTarget.z));

            Quaternion spineRot = spine.rotation * Quaternion.Euler(relativeVec);
            spine.rotation = spineRot;
        }

        //if (Physics.Raycast(BulletPos.transform.position, BulletPos.transform.forward, out hit, 30))
        //{
        //    Debug.Log(hit.collider.gameObject.name);
        //    Debug.DrawLine(lookTarget, hit.point, Color.green);
        //}

    }
    void GrednadeThrow()
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
        }

        items[itemIndex].itemGameObject.SetActive(false);
        anim.SetLayerWeight(itemIndex, 1);

        //----
        StartCoroutine(DelaySwap(itemIndex));

        if (preItemIndex != -1)
        {
            items[preItemIndex].itemGameObject.SetActive(false);
            anim.SetLayerWeight(preItemIndex, 0);
        }

        preItemIndex = itemIndex;

        if(PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }

    }

    IEnumerator DelaySwap(int index)
    {
        yield return new WaitForSeconds(0.5f);
   

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(!PV.IsMine && targetPlayer == PV.Owner)
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

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        playerManager.Die();
    }

    public void BindPlayerCamera(Camera camera)
    {
        playerCamera = camera;
    }

   

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
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
        if(!IsSwapDelay)
            Grenade();

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
        Debug.Log("ReloadStart");
        IsReloading = true;
    }
    void OnReloadingEnd()
    {
        ReloadImage.ReloadEnd();
        Debug.Log("ReloadEnd");
        IsReloading = false;
    }

    void OnShootingStart()
    {
        Aiming = true;
        
    }

}

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable, IPunObservable
{
    [SerializeField] float silencespeed, walkSpeed,crouchingSpeed ,jumpForce, smoothTime;
    ReloadCursor ReloadImage;
    //애니메이터 
    bool IsSwapDelay = false;
    bool IsReloading = false;
    bool Crouching = false;
    //-----------------------
    [SerializeField] Item[] items;
    int itemIndex = 0;
    int preItemIndex = -1;

    bool isGrounded;
    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;

    const float maxHealth = 100.0f;
    float currentHealth = maxHealth;

    [HideInInspector] public Camera playerCamera;
    Rigidbody rb;
    PhotonView PV;
    PlayerManager playerManager;
    Animator anim;
    Transform spine;

    float MaxYAxis = 2.5f;
    Vector3 relativeVec = new Vector3(0, -55, -100);

    Vector2 AnimControlVelocity = Vector2.zero;
    float CrouchingDecreaseFactor = 0.01f;
    float DecreaseFactor = 0.1f;
    float MaxAnimVelocity = 1.0f;

    // For Spine Rotation Sync
    Vector3 lookTarget = Vector3.zero;

    // For Lag Compensation
    Vector3 networkPosition = Vector3.zero;
    Quaternion networkRotation = Quaternion.identity;

    //---------------Grenade 
    [SerializeField]float throwVelocity;

    [SerializeField]
    GameObject grenade , GrenadeOrbit;
    [SerializeField]
    Transform throwPoint;

    private void Awake()
    {
        ReloadImage = GameObject.Find("Reload").GetComponent<ReloadCursor>();
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
        spine = anim.GetBoneTransform(HumanBodyBones.Spine);
        
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();


        /// grenade
        GrenadeOrbit.SetActive(false);
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

    private void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }
        
        Move();
        Jump();
        SwapWeapon();



        if (itemIndex == 1 || itemIndex == 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsReloading && !IsSwapDelay)
                {
                    items[itemIndex].Use();
                }
                else
                {

                }
            }
        }

        if(Input.GetKeyDown(KeyCode.R) && !anim.GetBool("IsReloading"))
        {
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
            //anim.SetBool("Crouching", true);
        }
        else
        {
            Crouching = false;
            // anim.SetBool("Crouching", false);
        }

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? silencespeed : walkSpeed),
            ref smoothMoveVelocity, smoothTime);

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftControl) ? crouchingSpeed : walkSpeed),
           ref smoothMoveVelocity, smoothTime); 


        AnimControlVelocity.x += moveAmount.x * Time.deltaTime * 2;
        AnimControlVelocity.y += moveAmount.z * Time.deltaTime;

        AnimControlVelocity.x = Mathf.Clamp(AnimControlVelocity.x, -MaxAnimVelocity, MaxAnimVelocity);
        AnimControlVelocity.y = Mathf.Clamp(AnimControlVelocity.y, -MaxAnimVelocity, MaxAnimVelocity);

        if(moveDir.x == 0 && AnimControlVelocity.x > 0)
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
            if(PV.IsMine && hit.collider.tag == "Player") 
                return;

            lookTarget = hit.point;
            lookTarget.y = Mathf.Clamp(lookTarget.y, 0, MaxYAxis);
            spine.LookAt(lookTarget);
            transform.LookAt(new Vector3(lookTarget.x, 0, lookTarget.z));

            Quaternion spineRot = spine.rotation * Quaternion.Euler(relativeVec);
            spine.rotation = spineRot;
        }
        if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, 8))
        {


            Debug.Log(hit.collider.gameObject.name);
            Debug.DrawLine(lookTarget, hit.point, Color.green);


        }

    }
    void GrednadeThrow()
    {

        if (Input.GetMouseButton(0) && !anim.GetBool("Throw"))
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
        Vector3 nextVec = transform.forward * throwVelocity;
        nextVec.y = 5.0f;
        GameObject instanceGrenade = Instantiate(grenade, throwPoint.transform.position, throwPoint.transform.rotation);
        Rigidbody rigidGrenade = instanceGrenade.GetComponent<Rigidbody>();
        rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
        rigidGrenade.AddTorque(Vector3.back * 5, ForceMode.Impulse);// 회전
    }
    void OnThrowStart()
    {
        //var clone = Instantiate(grenade);
        //this.simul.Shoot(clone, startPoint.position, endPoint.position, g, heightGo.position.y);
        Grenade();
        GrenadeOrbit.SetActive(false);
    }

  
    public void SetGroundedState(bool grounded)
    {
        isGrounded = grounded;
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
        {
            rb.position = Vector3.MoveTowards(rb.position, networkPosition, Time.fixedDeltaTime);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, networkRotation, Time.fixedDeltaTime);
            return;
        }

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    void EquipItem(int _index)
    {
        if (_index == preItemIndex)
            return;

        anim.SetTrigger("Swap");

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(false);
        anim.SetLayerWeight(itemIndex, 1);
        
        if(preItemIndex != -1)
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
            // >> Lag Compensation
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.velocity);
            // <<

            stream.SendNext(lookTarget);
        }
        else
        {
            // >> Lag Compensation
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            rb.velocity     = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkPosition += rb.velocity * lag;
            // <<
            lookTarget = (Vector3)stream.ReceiveNext();
        }
    }

    public Player GetPhotonViewOwner()
    {
        return PV.Owner;
    }



    // 애니메이션 이벤트
    void OnThrowEnd()
    {
        if (itemIndex == 2)
            GrenadeOrbit.SetActive(true);
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

        IsSwapDelay = false;
        items[itemIndex].itemGameObject.SetActive(true);
        Debug.Log("SwapEnd");
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
}

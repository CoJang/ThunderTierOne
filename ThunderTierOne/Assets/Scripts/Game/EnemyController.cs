using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviourPun, IDamageable
{
    #region GamePlay Variables
    [SerializeField] const float maxHealth = 100.0f;
    [SerializeField] float currentHealth = maxHealth;

    [SerializeField] float SightRange = 10.0f;
    [SerializeField] float EnemyDelayTime = 1.0f;
    [SerializeField] float RotateSpeed = 30.0f;
    #endregion

    #region Enemy State
    enum EnemyState
    {
        DIE = 0,
        IDLE,
        RUN,
        SHOOT,
        DELAYED,
    }

    bool isSpotSomething = false;
    EnemyState currentState = EnemyState.IDLE;
    #endregion

    float[] dists;
    [SerializeField] List<PlayerController> targetPlayers;
    Transform lastTarget;

    PhotonView PV;
    NavMeshAgent agent;
    BotWeapon weapon;

    [SerializeField] Transform GunHandle;

    #region Animator Variables
    Animator anim;
    int runningHash = Animator.StringToHash("Running");
    int isDieHash   = Animator.StringToHash("IsDie");
    int shootHash   = Animator.StringToHash("ShootTrigger");
    int aimHash     = Animator.StringToHash("Aim");
    #endregion

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        weapon = GetComponentInChildren<BotWeapon>();

        targetPlayers.Capacity = 4;

        StartCoroutine(FindAllPlayers(4.0f));
        StartCoroutine(DetectPlayers(0.1f));
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.DELAYED || currentState == EnemyState.DIE) 
            return;

        if(agent.velocity.magnitude > 0.1f)
        {
            anim.SetBool(runningHash, true);
            currentState = EnemyState.RUN;
        }
        else
        {
            anim.SetBool(runningHash, false);

            if(!isSpotSomething)
            {
                currentState = EnemyState.IDLE;
                anim.SetBool(aimHash, false);
            }
            else
            {
                anim.SetBool(aimHash, true);

                if (currentState != EnemyState.DELAYED)
                {
                    currentState = EnemyState.SHOOT;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (targetPlayers == null || lastTarget == null) return;
        if (currentState == EnemyState.DIE)
            return;

        if (isSpotSomething)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastTarget.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
        }

        if (currentState == EnemyState.DELAYED)
            return;

        if (currentState == EnemyState.SHOOT)
        {
            Shoot();
        }
    }

    IEnumerator FindAllPlayers(float delayTime)
    {
        yield return new WaitForSecondsRealtime(delayTime);

        Debug.Log("FindAllPlayers");
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        targetPlayers = players.OfType<PlayerController>().ToList();
        //targetPlayers = (PlayerController[])players.Clone();

        dists = new float[targetPlayers.Count];

        StartCoroutine(FindAllPlayers(delayTime));
    }

    IEnumerator DetectPlayers(float occurPeriod)
    {
        yield return new WaitForSecondsRealtime(occurPeriod);

        if (targetPlayers == null || targetPlayers.Count == 0)
        {
            StartCoroutine(DetectPlayers(occurPeriod));
            yield break;
        }

        for(int i = 0; i < targetPlayers.Count; i++)
        {
            if(targetPlayers[i] == null)
            {
                targetPlayers.RemoveAt(i--);
                continue;
            }

            dists[i] = Vector3.Distance(targetPlayers[i].gameObject.transform.position, transform.position);
        }

        float minDist = Mathf.Min(dists);

        if (minDist > SightRange || targetPlayers.Count == 0)
        {
            isSpotSomething = false;
            StartCoroutine(DetectPlayers(occurPeriod));
            yield break;
        }


        for (int i = 0; i < targetPlayers.Count; i++)
        {
            if(minDist == dists[i])
            {
                lastTarget = targetPlayers[i].gameObject.transform;
                agent.SetDestination(lastTarget.position);
                //Debug.Log("Player is In Sight");
                isSpotSomething = true;
                StartCoroutine(DetectPlayers(occurPeriod));
            }
        }
    }

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + "Hit!");

        if (currentHealth <= 0)
        {
            if (currentState != EnemyState.DIE)
                Die();
        }
    }

    void Die()
    {
        currentState = EnemyState.DIE;
        anim.SetTrigger(isDieHash);
        StopAllCoroutines();

        agent.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
    }

    void Shoot()
    {
        //transform.LookAt(lastTarget);
        weapon.Use(lastTarget.position);
        anim.SetTrigger(shootHash);

        currentState = EnemyState.DELAYED;
        StartCoroutine(DeleyedState(EnemyDelayTime));
    }

    IEnumerator DeleyedState(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        currentState = EnemyState.IDLE;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (currentState == EnemyState.DIE) return;

        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        anim.SetIKPosition(AvatarIKGoal.LeftHand, GunHandle.position);
        anim.SetIKRotation(AvatarIKGoal.LeftHand, GunHandle.rotation);
    }

    public void OnCollisionEnter(Collision collision)
    {
        switch (collision.transform.tag)
        {
            case "Bullet":
                Debug.Log("Enemey Hit");
                TakeDamage(25);
                break;
        }
    }
}

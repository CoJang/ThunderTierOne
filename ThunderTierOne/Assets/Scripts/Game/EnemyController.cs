using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviourPun, IDamageable
{
    #region GamePlay Variables
    [SerializeField] const float maxHealth = 100.0f;
    [SerializeField] float currentHealth = maxHealth;

    [SerializeField] float SightRange = 10.0f;
    [SerializeField] float EnemyDelayTime = 1.0f;
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
    #endregion

    float[] dists;
    PlayerController[] targetPlayers;
    Transform lastTarget;

    PhotonView PV;
    NavMeshAgent agent;
    BotWeapon weapon;
    Transform spine;

    [SerializeField] Item[] items;
    [SerializeField] Transform GunHandle;

    EnemyState currentState = EnemyState.IDLE;

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
        //spine = anim.GetBoneTransform(HumanBodyBones.Spine);

        StartCoroutine(FindAllPlayers(5.0f));
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.DELAYED) return;

        if(agent.velocity.magnitude > 0.25f)
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
        if (targetPlayers == null) return;

        if(targetPlayers.Length == 0)
        {
            StartCoroutine(FindAllPlayers(0.0f));
        }

        if(currentState == EnemyState.SHOOT)
        {
            Shoot();
        }

        //Quaternion spineRot = spine.rotation * Quaternion.Euler(0, 30, 0);
        //spine.rotation = spineRot;
    }

    IEnumerator FindAllPlayers(float delayTime)
    {
        yield return new WaitForSecondsRealtime(delayTime);

        if (targetPlayers == null || targetPlayers.Length == 0)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            targetPlayers = new PlayerController[players.Length];
            targetPlayers = players;

            dists = new float[targetPlayers.Length];

            if(delayTime > 0)
                StartCoroutine(DetectPlayers(0.1f));
        }
    }

    IEnumerator DetectPlayers(float occurPeriod)
    {
        yield return new WaitForSecondsRealtime(occurPeriod);

        for(int i = 0; i < targetPlayers.Length; i++)
        {
            dists[i] = Vector3.Distance(targetPlayers[i].gameObject.transform.position, gameObject.transform.position);
        }

        float minDist = Mathf.Min(dists);

        if (minDist > SightRange)
        {
            StartCoroutine(DetectPlayers(occurPeriod));
            isSpotSomething = false;
            yield break;
        }


        for (int i = 0; i < dists.Length; i++)
        {
            if(minDist == dists[i])
            {
                lastTarget = targetPlayers[i].gameObject.transform;
                agent.SetDestination(lastTarget.position);
                Debug.Log("Player is In Sight");
                isSpotSomething = true;
                StartCoroutine(DetectPlayers(occurPeriod));
            }
        }
    }

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.Others, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            if (currentState != EnemyState.DIE)
                Die();
        }
    }

    void Die()
    {
        anim.SetBool(isDieHash, true);
        //PhotonNetwork.Destroy(gameObject);
    }

    void Shoot()
    {
        transform.LookAt(lastTarget);
        weapon.Use(lastTarget.position);
        //anim.SetTrigger(shootHash);

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
        // IK를 사용하여 왼손의 위치와 회전을 총의 오른쪽 손잡이에 맞춘다
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        anim.SetIKPosition(AvatarIKGoal.LeftHand, GunHandle.position);
        //anim.SetIKRotation(AvatarIKGoal.LeftHand, GunHandle.rotation);
    }
}

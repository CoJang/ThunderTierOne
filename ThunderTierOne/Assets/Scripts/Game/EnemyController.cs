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
    #endregion

    #region Enemy State
    enum EnemyState
    {
        DIE = 0,
        IDLE,
        RUN,
        SHOOT,
    }

    bool isSpotSomeThing = false;
    #endregion

    float[] dists;
    PlayerController[] targetPlayers;
    Transform lastTarget;

    PhotonView PV;
    NavMeshAgent agent;

    [SerializeField] Item[] items;

    EnemyState currentState = EnemyState.IDLE;

    #region Animator Variables
    Animator anim;
    int runningHash = Animator.StringToHash("Running");
    int isDieHash   = Animator.StringToHash("IsDie");
    int shootHash   = Animator.StringToHash("Shoot");
    #endregion

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        StartCoroutine(FindAllPlayers(5.0f));
    }

    void FixedUpdate()
    {
        if(agent.velocity.magnitude > 0.25f)
        {
            anim.SetBool(runningHash, true);
            currentState = EnemyState.RUN;
        }
        else
        {
            anim.SetBool(runningHash, false);

            if(!isSpotSomeThing)
                currentState = EnemyState.IDLE;
            else
                currentState = EnemyState.SHOOT;
        }
    }

    void LateUpdate()
    {
        if(currentState == EnemyState.SHOOT)
        {
            Shoot();
        }
    }

    IEnumerator FindAllPlayers(float delayTime)
    {
        yield return new WaitForSecondsRealtime(delayTime);

        if (targetPlayers == null)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            targetPlayers = new PlayerController[players.Length];
            targetPlayers = players;

            dists = new float[targetPlayers.Length];

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
            isSpotSomeThing = false;
            yield break;
        }


        for (int i = 0; i < dists.Length; i++)
        {
            if(minDist == dists[i])
            {
                lastTarget = targetPlayers[i].gameObject.transform;
                agent.SetDestination(lastTarget.position);
                Debug.Log("Player is In Sight");
                isSpotSomeThing = true;
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
        PhotonNetwork.Destroy(gameObject);
    }

    void Shoot()
    {

    }
}

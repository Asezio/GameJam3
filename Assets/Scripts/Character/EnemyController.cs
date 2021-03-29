using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD, PATROL, CHASE, DEAD }

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]

public class EnemyController : MonoBehaviour, IEndGameObserver
{
    private EnemyStates enemyStates;

    private NavMeshAgent agent;

    private Animator anim;

    protected CharacterStats characterStats;

    private Collider coll;

    [Header("Basic Settings")]
    public float sightRadius;
    private float speed;
    public bool isGuard;
    protected GameObject attackTarget;
    public float lookAtTime;
    private float ramainLookAtTime;
    private float lastAttackTime;
    private float lastSkillTime;
    private Quaternion guardRotation;

    [Header("Patrol Range")]
    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 guardPos;

    //bool配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;

    bool playerDead = false;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        ramainLookAtTime = lookAtTime;
    }
    private void Start()
    {
        if(isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
        //场景切换后修改
        GameManager.Instance.AddObserver(this);
    }

    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }
    private void Update()
    {
        if (characterStats.CurrentHealth == 0)
            isDead = true;
        if (playerDead == false)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
    }

    void SwitchAnimation()
    {
        anim.SetBool("walk", isWalk);
        anim.SetBool("chase", isChase);
        anim.SetBool("follow", isFollow);
        anim.SetBool("criticle", characterStats.isCritical);
        anim.SetBool("death", isDead);
    }

    void SwitchStates()
    {
        if (isDead)
            enemyStates = EnemyStates.DEAD;

        //发现Player，切换到CHASE
        else if(FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
        }

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                Guard();
                break;
            case EnemyStates.PATROL:
                Patrol();
                break;
            case EnemyStates.CHASE:
                ChasePlayer();
                break;
            case EnemyStates.DEAD:
                Dead();
                break;
        }
    }


    void ChasePlayer()
    {
        isWalk = false;
        isChase = true;
        agent.speed = speed;
        if(!FoundPlayer())
        {
            //拉脱仇恨回到上一个状态
            isFollow = false;
            if(ramainLookAtTime > 0)
            {
                agent.destination = transform.position;
                ramainLookAtTime -= Time.deltaTime;
            }
            else if(isGuard)
            {
                enemyStates = EnemyStates.GUARD;
            }
            else
            {
                enemyStates = EnemyStates.PATROL;
            }

        }
        else
        {
            isFollow = true;
            agent.isStopped = false;
            agent.destination = attackTarget.transform.position;
        }

        if (TargetInAttackRange() || TargetInSkillRange())
        {
            isFollow = false;
            agent.isStopped = true;

            if (lastAttackTime < 0)
            {
                lastAttackTime = characterStats.attackData.coolDown;
                //暴击判断
                characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                //执行攻击
                Attack();
            }
        }
    }

    void Guard()
    {
        isChase = false;
        if (transform.position != guardPos)
        {
            isWalk = true;
            agent.isStopped = false;
            agent.destination = guardPos;

            if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
            {
                isWalk = false;
                transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
            }
        }
    }
    void Patrol()
    {
        isChase = false;
        agent.speed = speed * 0.5f;
        //判断是否到了目标点
        if(Vector3.Distance(wayPoint,transform.position) <= agent.stoppingDistance)
        {
            isWalk = false;
            if(ramainLookAtTime > 0)           
                ramainLookAtTime -= Time.deltaTime;
            else
                GetNewWayPoint();
        }
        else
        {
            isWalk = true;
            agent.destination = wayPoint;
        }
    }

    void Dead()
    {
        coll.enabled = false;
        agent.radius = 0;

        Destroy(gameObject, 2f);
    }
    void GetNewWayPoint()
    {
        ramainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ =Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);

        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if(TargetInAttackRange())
        {
            //近身攻击动画
            anim.SetTrigger("attack");
        }

        if(TargetInSkillRange())
        {
            //技能攻击动画
            anim.SetTrigger("skill");
        }
    }

    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else
            return false;
    }

    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }

    void Hit()
    {
        if(attackTarget!=null && transform.IsFacingTarget(attackTarget.transform)) 
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    public void EndNotify()
    {
        //获胜动画
        //停止移动
        //停止Agent
        isChase = false;
        isWalk = false;
        attackTarget = null;
        anim.SetBool("win", true);
        playerDead = true;
    }
}

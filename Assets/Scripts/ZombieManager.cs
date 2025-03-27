using System;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public enum EZombieState
{
    Patrol, //순찰모드
    Chase,
    Attack,
    Evade, //도망
    Damage,
    Idle,
    Die
}

public class ZombieManager : MonoBehaviour
{
    public EZombieState currentState = EZombieState.Patrol;
    public float attackRange = 1.0f; //공격 범위 
    public float attackDelay = 2.0f; //공격 딜레이
    private float nextAttackTime = 0.0f; //다음 공격 시간 관리
    public Transform[] patrolPoints; //순찰 경로 지점들
    private int currentPoint = 0; //현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f; //이동 속도
    public float trackingRange = 5.0f; //추적 범위 설정
    private bool isAttack = false; //공격 상태
    private float evadeRange = 5.0f; //도망 상태 회피 거리
    public float zombieHp = 10.0f;
    private float distanceToTarget; //Target 과의 거리 계산 값
    private bool isWaiting = false; //상태 전환 후 대기 상태 여부
    public float idleTime = 2.0f; //각 상태 전환 후 대기 시간
    private Coroutine stateCoroutine; //현재 실행 중인 코루틴 정보 저장

    Animator animator;

    private NavMeshAgent agent;

    private bool isJumping = false;
    private Rigidbody rb;
    public float jumpHeight = 2.0f;
    public float jumpDuration = 1.0f;
    private NavMeshLink[] navMeshLinks;

    private void Update()
    {
    }

    private void Start()
    {
        zombieHp = 10.0f;

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb= GetComponent<Rigidbody>();
        if(rb==null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;

        navMeshLinks = FindObjectsByType<NavMeshLink>(FindObjectsSortMode.None);

        ChangeState(currentState);
    }

    public void ChangeState(EZombieState newState)
    {
        if (isJumping) return; //점프했을 때 다른 행동 하지말라고 

        if (stateCoroutine != null)
        {
            StopCoroutine(stateCoroutine);
        }
        currentState = newState;

        switch (currentState)
        {
            case EZombieState.Idle:
                stateCoroutine = StartCoroutine(Idle());
                break;
            case EZombieState.Patrol:
                stateCoroutine = StartCoroutine(Patrol());
                break;
            case EZombieState.Chase:
                stateCoroutine = StartCoroutine(Chase());
                break;
            case EZombieState.Attack:
                stateCoroutine = StartCoroutine(Attack());
                break;
            case EZombieState.Evade:
                stateCoroutine = StartCoroutine(Evade());
                break;
            case EZombieState.Damage:
                //stateCoroutine = StartCoroutine(TakeDamage());
                break;
            case EZombieState.Die:
                stateCoroutine = StartCoroutine(Die());
                break;
        }
    }

    private IEnumerator Idle()
    {
        Debug.Log(gameObject.name + " : 대기");
        animator.Play("ZombieIdle");
        SoundManager.Instance.PlaySFX("ZombieBreathing", transform.position);

        while (currentState == EZombieState.Idle)
        {
            agent.isStopped = true;

            animator.SetBool("isRun", false);

            float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

            if (distance < trackingRange)
            {
                ChangeState(EZombieState.Chase);
            }
            else if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }
        yield return null;
        }
    }

    private IEnumerator Patrol()
    {
        
        Debug.Log(gameObject.name + " : 순찰 중");
        SoundManager.Instance.PlaySFX("ZombieBreathing", transform.position);
        while (currentState == EZombieState.Patrol)
        {
            if (patrolPoints.Length > 0)
            {
                animator.SetBool("isRun", true);

                Transform targetPoint = patrolPoints[currentPoint];
                Vector3 direction = targetPoint.position - transform.position;
                
                agent.isStopped = false;
                agent.speed = moveSpeed;
                agent.destination = targetPoint.position;

                if (agent.isOnOffMeshLink)
                {
                    StartCoroutine(JumpAcrossLink());
                }

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
                if (distance < trackingRange)
                {
                    
                    ChangeState(EZombieState.Chase);
                }
            }
        yield return null;
        }
    }

    private IEnumerator Chase()
    {
        Debug.Log(gameObject.name + " : 쫓는 중");
        SoundManager.Instance.PlaySFX("ZombieBreathing", transform.position);

        while (currentState == EZombieState.Chase)
        {
            animator.SetBool("isRun", true);

            float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

            Vector3 direction = PlayerManager.Instance.transform.position - transform.position;

            agent.speed = moveSpeed;
            agent.isStopped = false; 
            agent.destination = PlayerManager.Instance.transform.position;
                                                                                                                                                                

            if (distance > trackingRange)
            {
                ChangeState(EZombieState.Patrol);
            }

            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }
        yield return null;
        }
    }

    private IEnumerator Attack()
    {
        Debug.Log(gameObject.name + " : 공격하는 중");
        //transform.LookAt(target);
        animator.SetTrigger("Attack");
        SoundManager.Instance.PlaySFX("ZombieAttack", transform.position);

        //agent.isStopped = true;

        yield return new WaitForSeconds(attackDelay);

        float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);
        if(distance > attackRange)
        {
            ChangeState(EZombieState.Chase);
        }
        else
        {
            ChangeState(EZombieState.Attack);
        }
    }

    private IEnumerator Evade()
    {
        Debug.Log(gameObject.name + " : 도망가는 중");

        Vector3 evadeDirection = (transform.position - PlayerManager.Instance.transform.position).normalized;
        float evadeTime = 3.0f;
        float timer = 0.0f;

        //타겟을 바라보고 가는 것이 아니라 방향을 지정해서 가는 것이라 LookAt함수 사용X
        //Quaternion targetRotation = Quaternion.LookRotation(evadeDirection); 
        //transform.rotation = targetRotation;

        while (currentState == EZombieState.Evade && timer < evadeTime)
        {
            animator.SetBool("isRun", true);

            //transform.position += evadeDirection * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;

            agent.speed = moveSpeed;
            agent.isStopped = false;
            agent.destination = evadeDirection;

            yield return null;
        }

        ChangeState(EZombieState.Idle);
    }

    public IEnumerator TakeDamage(float damage) //매개변수 추가해서 부위별 데미지 가능 
    {
        Debug.Log(gameObject.name + " : 아야");
        animator.SetTrigger("Damage");
        SoundManager.Instance.PlaySFX("ZombieDamage", transform.position);
        zombieHp -= damage;

        agent.isStopped = true;

        if(zombieHp <= 0)
        {
            ChangeState(EZombieState.Die);
        }
        else
        {
            ChangeState(EZombieState.Chase);
        }
        yield return null;
    }
    private IEnumerator Die()
    {
        //SoundManager.Instance.StopSFX();
        agent.isStopped = true;

        Debug.Log(gameObject.name + " : 죽음");
        animator.SetTrigger("Die");
        
        yield return new WaitForSeconds(6f);
        gameObject.SetActive(false);
    }

    private IEnumerator JumpAcrossLink()
    {
        isJumping = true;

        agent.isStopped = true;

        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;

        //포물선을 그리며 점프하도록 계산
        float elapsedTime = 0.0f;
        while(elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * MathF.PI) * jumpHeight; //포물선 경로
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //도착점에 위치함
        transform.position = endPos;
        //NavMeshAgent 경로 재개 
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isJumping = false;
    }
}

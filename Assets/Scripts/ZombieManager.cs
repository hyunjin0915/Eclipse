using System;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public enum EZombieState
{
    Patrol, //�������
    Chase,
    Attack,
    Evade, //����
    Damage,
    Idle,
    Die
}

public class ZombieManager : MonoBehaviour
{
    public EZombieState currentState = EZombieState.Patrol;
    public float attackRange = 1.0f; //���� ���� 
    public float attackDelay = 2.0f; //���� ������
    private float nextAttackTime = 0.0f; //���� ���� �ð� ����
    public Transform[] patrolPoints; //���� ��� ������
    private int currentPoint = 0; //���� ���� ��� ���� �ε���
    public float moveSpeed = 2.0f; //�̵� �ӵ�
    public float trackingRange = 5.0f; //���� ���� ����
    private bool isAttack = false; //���� ����
    private float evadeRange = 5.0f; //���� ���� ȸ�� �Ÿ�
    public float zombieHp = 10.0f;
    private float distanceToTarget; //Target ���� �Ÿ� ��� ��
    private bool isWaiting = false; //���� ��ȯ �� ��� ���� ����
    public float idleTime = 2.0f; //�� ���� ��ȯ �� ��� �ð�
    private Coroutine stateCoroutine; //���� ���� ���� �ڷ�ƾ ���� ����

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
        if (isJumping) return; //�������� �� �ٸ� �ൿ ��������� 

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
        Debug.Log(gameObject.name + " : ���");
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
        
        Debug.Log(gameObject.name + " : ���� ��");
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
        Debug.Log(gameObject.name + " : �Ѵ� ��");
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
        Debug.Log(gameObject.name + " : �����ϴ� ��");
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
        Debug.Log(gameObject.name + " : �������� ��");

        Vector3 evadeDirection = (transform.position - PlayerManager.Instance.transform.position).normalized;
        float evadeTime = 3.0f;
        float timer = 0.0f;

        //Ÿ���� �ٶ󺸰� ���� ���� �ƴ϶� ������ �����ؼ� ���� ���̶� LookAt�Լ� ���X
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

    public IEnumerator TakeDamage(float damage) //�Ű����� �߰��ؼ� ������ ������ ���� 
    {
        Debug.Log(gameObject.name + " : �ƾ�");
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

        Debug.Log(gameObject.name + " : ����");
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

        //�������� �׸��� �����ϵ��� ���
        float elapsedTime = 0.0f;
        while(elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            currentPosition.y += Mathf.Sin(t * MathF.PI) * jumpHeight; //������ ���
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //�������� ��ġ��
        transform.position = endPos;
        //NavMeshAgent ��� �簳 
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isJumping = false;
    }
}

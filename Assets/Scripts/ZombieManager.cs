using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
    public EZombieState currentState = EZombieState.Idle;
    public Transform target;
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

    public AudioSource audioSource;
    public AudioClip audioClipIdle;
    public AudioClip audioClipAttack;
    public AudioClip audioClipDamage;

    private NavMeshAgent agent;

    private void Update()
    {
        //distanceToTarget = Vector3.Distance(transform.position, target.position);
    }

    private void Start()
    {
        zombieHp = 10.0f;

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();

        ChangeState(currentState);
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //other.gameObject.GetComponentInChildren<SkinnedMeshRenderer>()
            other.gameObject.GetComponent<PlayerManager>().WeaponChangeSoundOn();
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Damage");
            }

            other.gameObject.transform.position = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }
*/
    public void ChangeState(EZombieState newState)
    {
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
        audioSource.PlayOneShot(audioClipIdle);

        while (currentState == EZombieState.Idle)
        {
            agent.isStopped = true;

            animator.SetBool("isRun", false);

            float distance = Vector3.Distance(transform.position, target.position);

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
        audioSource.PlayOneShot(audioClipIdle);
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
                

                if (Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
                {
                    currentPoint = (currentPoint + 1) % patrolPoints.Length;
                }

                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < trackingRange)
                {
                    
                    ChangeState(EZombieState.Chase);
                }
                /*else if (distance < attackRange)
                {
                    animator.SetBool("isRun", false);
                    ChangeState(EZombieState.Attack);
                }*/
            }
        yield return null;
        }
    }

    private IEnumerator Chase()
    {
        Debug.Log(gameObject.name + " : �Ѵ� ��");
        audioSource.PlayOneShot(audioClipIdle);

        while (currentState == EZombieState.Chase)
        {
            animator.SetBool("isRun", true);

            float distance = Vector3.Distance(transform.position, target.position);

            Vector3 direction = target.position - transform.position;
            //transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            //transform.LookAt(target.transform);

            agent.speed = moveSpeed;
            agent.isStopped = false; 
                                                                                                                                                                

            if (distance > trackingRange)
            {
                ChangeState(EZombieState.Idle);
            }

            if (distance < attackRange)
            {
                ChangeState(EZombieState.Attack);
            }
            /*if(distance < evadeRange)
            {
                ChangeState(EZombieState.Evade);
            }*/
        yield return null;
        }
    }

    private IEnumerator Attack()
    {
        Debug.Log(gameObject.name + " : �����ϴ� ��");
        //transform.LookAt(target);
        animator.SetTrigger("Attack");
        audioSource.PlayOneShot(audioClipAttack);

        //agent.isStopped = true;

        yield return new WaitForSeconds(attackDelay);

        float distance = Vector3.Distance(transform.position, target.position);
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

        Vector3 evadeDirection = (transform.position - target.position).normalized;
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
        audioSource.PlayOneShot(audioClipDamage);
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
        agent.isStopped = true;

        Debug.Log(gameObject.name + " : ����");
        animator.SetTrigger("Die");
        
        yield return new WaitForSeconds(6f);
        gameObject.SetActive(false);
    }
}

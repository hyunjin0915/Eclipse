using UnityEngine;

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
    private float trackingRange = 5.0f; //���� ���� ����
    private bool isAttack = false; //���� ����
    private float evadeRange = 5.0f; //���� ���� ȸ�� �Ÿ�
    private float zombieHp = 10.0f;
    private float distanceToTarget; //Target ���� �Ÿ� ��� ��
    private bool isWaiting = false; //���� ��ȯ �� ��� ���� ����
    public float idleTime = 2.0f; //�� ���� ��ȯ �� ��� �ð�

    private void Update()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < trackingRange)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            transform.LookAt(target.position);

            currentState = EZombieState.Chase;
            if (distanceToTarget < attackRange)
            {
                currentState = EZombieState.Attack;
                Debug.Log("����");
            }
        }
        else if (patrolPoints.Length > 0)
        {
            //Debug.Log("���� ��");
            Transform targetPoint = patrolPoints[currentPoint];
            Vector3 direction = (targetPoint.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(targetPoint.position);

            if(Vector3.Distance(transform.position, targetPoint.position) < 0.3f)
            {
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
            }
        }

    }

    private void Start()
    {

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

}

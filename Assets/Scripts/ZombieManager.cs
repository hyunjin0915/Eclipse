using UnityEngine;

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
    public EZombieState currentState = EZombieState.Idle;
    public Transform target;
    public float attackRange = 1.0f; //공격 범위 
    public float attackDelay = 2.0f; //공격 딜레이
    private float nextAttackTime = 0.0f; //다음 공격 시간 관리
    public Transform[] patrolPoints; //순찰 경로 지점들
    private int currentPoint = 0; //현재 순찰 경로 지점 인덱스
    public float moveSpeed = 2.0f; //이동 속도
    private float trackingRange = 5.0f; //추적 범위 설정
    private bool isAttack = false; //공격 상태
    private float evadeRange = 5.0f; //도망 상태 회피 거리
    private float zombieHp = 10.0f;
    private float distanceToTarget; //Target 과의 거리 계산 값
    private bool isWaiting = false; //상태 전환 후 대기 상태 여부
    public float idleTime = 2.0f; //각 상태 전환 후 대기 시간

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
                Debug.Log("공격");
            }
        }
        else if (patrolPoints.Length > 0)
        {
            //Debug.Log("순찰 중");
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

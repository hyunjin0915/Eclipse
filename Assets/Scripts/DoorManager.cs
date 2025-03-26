using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public bool isOpen = false;
    private Animator animator;

    public bool LastOpenedForward{get; private set;}=true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsPlayerInfront(Transform player)
    {
        //플레이어와 문 사이의 벡터 계산산
        Vector3 toPlayer = (player.position - transform.position).normalized;
        //문이 향하는 방향과 플레이어의 방향을비교(내적 연산)
        float dotProduct = Vector3.Dot(transform.forward, toPlayer);
        //0보다 크면 플레이어가 문 앞에 있음 
        return dotProduct>0;
    }

    public bool Open(Transform player)
    {
        if(!isOpen)
        {
            isOpen = true; //문이 열린 상태로 설정정

            if(IsPlayerInfront(player)) //플레이어가 앞에 있으면 정방향 애니 재생생
            {
                animator.SetTrigger("OpenForward");
                LastOpenedForward = true; //문이 정방향으로 열림
            }
            else
            {
                animator.SetTrigger("OpenBackward");
                LastOpenedForward = false;
            }
            return true;
        }
        return false;
    }

    public void CloseForward(Transform player)
    {
        if(isOpen)
        {
            isOpen = false;
            animator.SetTrigger("CloseForward");
        } 
    }

    public void CloseBackward(Transform player)
    {
        if(isOpen)
        {
            isOpen = false;
            animator.SetTrigger("CloseBackward");
        }
    }
}

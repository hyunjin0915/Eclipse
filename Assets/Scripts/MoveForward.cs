using UnityEngine;

public class MoveForward : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    
    void Update()
    {
        // Z축 양의 방향으로 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}

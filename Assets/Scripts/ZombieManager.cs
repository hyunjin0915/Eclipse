using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
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

    
}

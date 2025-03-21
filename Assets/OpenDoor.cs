using UnityEngine;

public class OpenDoor : MonoBehaviour
{
   public  bool isNear = false;
   public  bool isOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isNear && !isOpen)
            {
                Debug.Log("¹®¿­¸²");
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3 (-90, 0, -90)), 0.1f);
                transform.localRotation = Quaternion.Euler(0, 90, 0);
                isOpen = true;
            }
            if (isNear && isOpen)
            {
                transform.localRotation = Quaternion.Euler(-90, 0, 0);
                isOpen = false;

            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isNear = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isNear = false;
        }
    }

}

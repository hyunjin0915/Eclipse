using System.Collections;
using UnityEngine;

public class SuppliesTrigger : MonoBehaviour
{
    public GameObject HeliPos;
    public GameObject SuppliesBox;
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            //SoundManager.Instance.PlaySFX("HeliPass", HeliPos.transform.position);
            SoundManager.Instance.PlaySFX("HeliPass");
            StartCoroutine(SuppliesBoxActive());
        }
    }
    
    IEnumerator SuppliesBoxActive()
    {
        yield return new WaitForSeconds(3f);
        SuppliesBox.SetActive(true);
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
}

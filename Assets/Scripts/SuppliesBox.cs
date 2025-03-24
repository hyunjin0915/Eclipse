using System.Collections.Generic;
using UnityEngine;

public class SuppliesBox : MonoBehaviour
{
    public List<GameObject> supplies = new List<GameObject>();

    public void PopSupplies()
    {
        foreach (GameObject item in supplies)
        {
            item.SetActive(true);
        }
    }
}

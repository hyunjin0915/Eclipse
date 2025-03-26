using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "HPScriptableObject", menuName = "ScriptableObjects/Health Manager")]
public class HPScriptableObject : ScriptableObject
{
    public int health = 0;
    public bool isHelathDecreas = false;
    [SerializeField]
    public int maxHealth = 100;

    public Action hpChangeAction;

    private void OnEnable()
    {
        health = maxHealth;
        /*if (healthChangeEvent == null)
        {
            healthChangeEvent = new Action;
        }*/
    }

    public void DecreaseHealth(int amount)
    {
        health -= amount;
        isHelathDecreas = true;
        hpChangeAction.Invoke();
        if(health < 0)
        {
            //게임오버이벤트발생
        }
    }

    public void IncreaseHealth(int amount)
    {
        health += amount;
        isHelathDecreas = false;
        hpChangeAction.Invoke();
        if (health > maxHealth) //최대 체력을 넘기면
        {
            health = maxHealth;
        }
    }
}

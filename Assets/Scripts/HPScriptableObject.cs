using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "HPScriptableObject", menuName = "ScriptableObjects/Health Manager")]
public class HPScriptableObject : ScriptableObject
{
    public int health = 0;

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
        hpChangeAction.Invoke();
        if(health < 0)
        {
            //���ӿ����̺�Ʈ�߻�
        }
    }

    public void IncreaseHealth(int amount)
    {
        health += amount;
        hpChangeAction.Invoke();
        if (health > maxHealth) //�ִ� ü���� �ѱ��
        {
            health = maxHealth;
        }
    }
}

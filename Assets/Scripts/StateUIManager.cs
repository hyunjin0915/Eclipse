using UnityEngine;
using UnityEngine.UI;

public class StateUIManager : MonoBehaviour
{
    public HPScriptableObject hpManager;
    public Image hpBar;
    float curHPRatio = 0.0f;

    private void OnEnable()
    {
        hpManager.hpChangeAction += HPUIUpate;
    }
    private void OnDisable()
    {
        hpManager.hpChangeAction -= HPUIUpate;
    }


    public void HPUIUpate()
    {
        curHPRatio = (float)hpManager.health / (float)hpManager.maxHealth;
        hpBar.fillAmount = curHPRatio;
    }
}

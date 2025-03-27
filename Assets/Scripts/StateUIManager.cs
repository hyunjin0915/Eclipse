using UnityEngine;
using UnityEngine.UI;

public class StateUIManager : MonoBehaviour
{
    public HPScriptableObject hpManager;
    public Image hpBar;
    public GameObject DyingPanel;

    float curHPRatio = 0.0f;

    private void OnEnable()
    {
        hpManager.hpChangeAction += HPUIUpate;
        hpManager.hpChangeAction += DyingPanelUpdate;
    }
    private void OnDisable()
    {
        hpManager.hpChangeAction -= HPUIUpate;
        hpManager.hpChangeAction -= DyingPanelUpdate;
    }


    public void HPUIUpate()
    {
        curHPRatio = (float)hpManager.health / (float)hpManager.maxHealth;
        hpBar.fillAmount = curHPRatio;
    }

    public void DyingPanelUpdate()
    {
        if(hpManager.health < 40)
        {
            DyingPanel.SetActive(true);
        }
        else
        {
            DyingPanel.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class InGameGPT : MonoBehaviour
{
    public PlayerDataScriptableObject playerData;
    public HPScriptableObject hpManager;

    public Text GPTText;

    void OnEnable()
    {
        hpManager.hpChangeAction += GPTAlarm;
    }
    void OnDisable()
    {
        hpManager.hpChangeAction -= GPTAlarm;
    }

    public void GPTAlarm()
    {
        string chatMessage = "";
        if(hpManager.isHelathDecreas)
        {
            chatMessage = "지금 접속한 유저 이름은"+ playerData.user_name + "이야." +
                "그리고 지금 게임을 플레이하다가 좀비한테 맞아서 체력이 감소되었어. 유저 이름을 넣고 해당 정보를 알려주는 알람 메세지를 재치있게 20자 내외로 작성해줘.";
        }
        else
        {
            chatMessage = "지금 접속한 유저 이름은"+ playerData.user_name + "이야." +
                "그리고 지금 게임을 플레이하다가 아이템을 먹어서 체력이 회복되었어. 유저 이름을 넣고 해당 정보를 알려주는 알람 메세지를 재치있게 20자 내외로 작성해줘.";
        }
        ChatGPTManager.Instance.AskChatGPT(chatMessage);
        Invoke("UpdateUI",2f);
    }
    private void UpdateUI()
    {
        GPTText.text = ChatGPTManager.Instance.chatResponse.Content;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Text GPTText;

    public PlayerDataScriptableObject playerData;

    private void Start()
    {
        MakeSomeJoke();
        SoundManager.Instance.PlayBGM("ZombieNEWS");
    }

    private void MakeSomeJoke()
    {
        string chatMessage = "지금 접속한 유저 이름은"+ playerData.user_name + "이야." +
                                "유저 이름을 넣은 간단한 유머를 섞어서 유저를 환영하는 말을 해줘. 시작은 반가워요 유니티부트캠프! 로 해줘.";
        ChatGPTManager.Instance.AskChatGPT(chatMessage);
        Invoke("UpdateUI",2f);
    }
    private void UpdateUI()
    {
        GPTText.text = ChatGPTManager.Instance.chatResponse.Content;
    }
    public void OnStartButton()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlaySFX("UIButton");
        //audioSource.PlayOneShot(audioClipClicked);
        SceneController.Instance.LoadScene("Stage1");
        //SceneManager.LoadSceneAsync("TutorialScene",LoadSceneMode.Single);
    }

    public void OnExitButton()
    {
        SoundManager.Instance.StopBGM();

        SoundManager.Instance.PlaySFX("UIButton");
        //audioSource.PlayOneShot(audioClipClicked);
        SceneController.Instance.ExitScene();
        //Application.Quit();
    }
}

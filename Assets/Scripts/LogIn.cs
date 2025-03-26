using UnityEngine;

public class LogIn : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayBGM("SmallAdvanture_BGM");
    }

    // Update is called once per frame
    void Update()
    {
        while(NetworkManager.Instance.messageQueue.TryDequeue(out string message))
        {
            if(message.Equals("success"))
            {
                SceneController.Instance.LoadScene("MenuScene");
                //LoadingManager.Instance.StartLoading("MenuScene");
            }
        }
    }
}

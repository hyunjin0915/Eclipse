using UnityEngine;

public class LogIn : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

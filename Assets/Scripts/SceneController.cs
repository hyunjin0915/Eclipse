using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);

        Debug.Log(sceneName + "���� ����");
    }

    public void ExitScene()
    {
        Application.Quit();
    }
 }

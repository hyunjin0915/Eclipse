using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
/*    public AudioSource audioSource;
    public AudioClip audioClipClicked;
*/
    private void Start()
    {
        SoundManager.Instance.PlayBGM("ZombieNEWS");
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

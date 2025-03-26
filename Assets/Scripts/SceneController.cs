using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : Singleton<SceneController>
{
    //public Image panel;
    //float fadeDuration = 1f;
    //public string nextSceneName;
    //private bool isFading = false;

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);

        Debug.Log(sceneName + "으로 변경");
    }

    public void ExitScene()
    {
        Application.Quit();
    }

    // private IEnumerator FadeInAndLoadScene()
    // {
    //     isFading = true;

    //     yield return StartCoroutine(FadeImage(0, 1, fadeDuration));

    //     SceneManager.LoadScene(nextSceneName);

    //     yield return StartCoroutine(FadeImage(1,0,fadeDuration));

    //     isFading = false;
    // }

    // private IEnumerator FadeImage(float startAlpha, float endAlpha, float duration)
    // {
    //     float elapsedTime = 0f;

    //     Color panelColor = panel.color;

    //     while(elapsedTime<duration)
    //     {
    //         elapsedTime += Time.deltaTime;
    //         float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime/duration);
    //         panelColor.a = newAlpha;
    //         panel.color = panelColor;
    //         yield return null;
    //     }
    //     panelColor.a=endAlpha;
    //     panel.color = panelColor;

    //     if(isFading)
    //     {
    //         SceneManager.LoadScene(nextSceneName);
    //     }
    // }
 }

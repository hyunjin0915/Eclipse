using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : Singleton<LoadingManager>
{
    public Slider loadingSlider;

    private string nextSceneName;
    
    public void StartLoading(string _sceneName)
    {
        nextSceneName = _sceneName;
        StartCoroutine(LoadLoadingSceneAndNextScene()); //다음 씬에 대한 정보 넣어주면 됨
    }
    IEnumerator LoadLoadingSceneAndNextScene()
    {
        AsyncOperation loadingSceneOP = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
        loadingSceneOP.allowSceneActivation =false; //자동으로 씬 전환하지 않도록 설정 

        while(!loadingSceneOP.isDone)
        {
            if(loadingSceneOP.progress >= 0.9f)
            {
                loadingSceneOP.allowSceneActivation = true;
            }
            yield return null;
        }
        FindLodadingSliderInScene();

        AsyncOperation nextSceneOp = SceneManager.LoadSceneAsync(nextSceneName);
        while(!nextSceneOp.isDone)
        {
            loadingSlider.value = nextSceneOp.progress;
            yield return null;
        }
        SceneManager.UnloadSceneAsync("LoadingScene");
    }

    void FindLodadingSliderInScene()
    {
        loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();
    }
}

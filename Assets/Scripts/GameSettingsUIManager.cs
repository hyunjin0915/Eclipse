using UnityEngine;
using UnityEngine.UI;

public class GameSettingsUIManager : MonoBehaviour
{
    public GameObject SettingsObj;
    private bool isActive = true;

    public Text resolutionText;
    public Text graphicsQualityText;
    public Text fullScreenText;

    private int resolutionIndex = 0;
    private int qualityIndex = 0;
    private bool isFullScreen = true;

    private string[] resolutions = { "1280 X 720", "1920 X 1080", "2560 X 1440", "3840 X 2160" };
    private string[] qualityOptions = { "Low", "Normal", "High" };


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSettings()
    {
        //소리 재생
        SettingsObj.SetActive(isActive);
        isActive = !isActive;
    }
    public void OnResolutionLeftClick()
    {
        resolutionIndex = Mathf.Max(0, resolutionIndex - 1);
        UpdateResolutionText();
    }

    public void OnResolutionRightClick()
    {
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);
        UpdateResolutionText();
    }
    public void OnGraphicsLeftClick()
    {
        qualityIndex = Mathf.Max(0, qualityIndex - 1);
        UpdateGraphicsText();
    }
    public void OnGraphicsRightClick()
    {
        qualityIndex = Mathf.Min(qualityOptions.Length - 1, qualityIndex + 1);
        UpdateGraphicsText();
    }
    public void OnFullScreenClick()
    {
        isFullScreen = !isFullScreen;
        UpdateFullScreenText();
    }
    //=====
    private void UpdateResolutionText()
    {
        resolutionText.text = resolutions[resolutionIndex];
    }
    private void UpdateGraphicsText()
    {
        graphicsQualityText.text = qualityOptions[qualityIndex];
    }
    private void UpdateFullScreenText()
    {
        fullScreenText.text = isFullScreen ? "ON" : "OFF";
    }
    public void OnApplySettingsClick()
    {
        ApplySettings();
        SaveSettings();
    }
    private void ApplySettings()
    {
        string[] res = resolutions[resolutionIndex].Split('X');
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, isFullScreen);
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex",resolutionIndex);
        PlayerPrefs.SetInt("GraphicsQualityIndex",qualityIndex);
        PlayerPrefs.SetInt("FullScreen",isFullScreen?1:0);
        PlayerPrefs.Save();
    }
    private void LoadSettings()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex",1);
        qualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex",1);
    }
}

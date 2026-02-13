using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;

    [SerializeField] private GameObject gameSettings;
    [SerializeField] private GameObject soundSettings;
    [SerializeField] private GameObject displaySettings;

    [SerializeField] private GameObject subtitleObj;

    [Header("Game Settings")]
    [SerializeField] private Slider lookSensitivitySlider;
    [SerializeField] private Toggle subtitleToggle;
    [SerializeField] private TMP_Dropdown subtitleSizeDropdown;

    [Header("Sound Settings")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider voiceSlider;

    [Header("Display Settings")]
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    public float sfxVolume { get; private set; }
    public float musicVolume { get; private set; }
    private float voiceVolume;

    private void OnEnable()
    {
        ShowGameSettings();
    }

    private void OnDisable()
    {
        ApplySoundSettings();
    }

    public void AddNativeResolution()
    {
        resolutionDropdown.options.Insert(0, new TMP_Dropdown.OptionData(Display.main.systemWidth + " x " + Display.main.systemHeight + " (Monitor Resolution)"));
        resolutionDropdown.value = 4;
    }

    public void ShowGameSettings()
    {
        gameSettings.SetActive(true);
        soundSettings.SetActive(false);
        displaySettings.SetActive(false);
        SetDisplayedSettingsToCurrent();
    }

    public void ShowSoundSettings()
    {
        gameSettings.SetActive(false);
        soundSettings.SetActive(true);
        displaySettings.SetActive(false);
        SetDisplayedSettingsToCurrent();
    }

    public void ShowDisplaySettings()
    {
        gameSettings.SetActive(false);
        soundSettings.SetActive(false);
        displaySettings.SetActive(true);
        SetDisplayedSettingsToCurrent();
    }

    public void OnSfxSliderValueChange(float value)
    {
        sfxVolume = value;
    }

    public void OnMusicSliderValueChange(float value)
    {
        if (audioManager)
        {
            audioManager.UpdateMixerValue("Music Volume", value);
            audioManager.UpdateMixerValue("Title Music Volume", value);
        }
        musicVolume = value;
    }

    public void OnVoiceSliderValueChange(float value)
    {
        if (audioManager)
            audioManager.UpdateMixerValue("Voice Volume", value);
        voiceVolume = value;
    }

    /*public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }*/

    public void ApplyGameSettings()
    {
        FPSController fpsController = FindFirstObjectByType<FPSController>();
        if (fpsController != null)
        {
            fpsController.lookSpeed = lookSensitivitySlider.value / 25;
        }
        PlayerPrefs.SetInt("LookSensitivity", (int)lookSensitivitySlider.value);

        if (subtitleObj != null)
            subtitleObj.SetActive(subtitleToggle.isOn);
        PlayerPrefs.SetInt("SubtitlesOn", subtitleToggle.isOn ? 1 : 0);

        if (subtitleObj != null) {
            switch (subtitleSizeDropdown.value)
            {
                case 0:
                    subtitleObj.GetComponent<TextMeshProUGUI>().fontSize = 18;
                    break;
                case 1:
                    subtitleObj.GetComponent<TextMeshProUGUI>().fontSize = 24;
                    break;
                case 2:
                    subtitleObj.GetComponent<TextMeshProUGUI>().fontSize = 30;
                    break;
            }
        }
        PlayerPrefs.SetInt("SubtitleSize", subtitleSizeDropdown.value);

        PlayerPrefs.Save();
    }

    public void ApplySoundSettings()
    {
        OnSfxSliderValueChange(sfxVolume);
        PlayerPrefs.SetInt("SFXVolume", (int)(sfxVolume * 100));
        OnMusicSliderValueChange(musicVolume);
        PlayerPrefs.SetInt("MusicVolume", (int)(musicVolume * 100));
        OnVoiceSliderValueChange(voiceVolume);
        PlayerPrefs.SetInt("VoiceVolume", (int)(voiceVolume * 100));

        PlayerPrefs.Save();
    }

    public void ApplyDisplaySettings()
    {
        QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("VSync", QualitySettings.vSyncCount);

        int horizontalRes = Int32.Parse(resolutionDropdown.options[resolutionDropdown.value].text.Split(' ')[0].Trim());
        int verticalRes = Int32.Parse(resolutionDropdown.options[resolutionDropdown.value].text.Split(' ')[2].Trim());
        Screen.SetResolution(horizontalRes, verticalRes, fullScreenToggle.isOn);
        PlayerPrefs.SetInt("ResolutionDropdownValue", resolutionDropdown.value);

        PlayerPrefs.Save();
    }

    public void ApplyCurrentSettings()
    {
        SetDisplayedSettingsToCurrent();
        ApplyGameSettings();
        ApplySoundSettings();
        ApplyDisplaySettings();
    }

    private void SetDisplayedSettingsToCurrent()
    {
        //Game
        lookSensitivitySlider.value = PlayerPrefs.GetInt("LookSensitivity", 50);

        subtitleToggle.isOn = PlayerPrefs.GetInt("SubtitlesOn", 1) == 1;

        subtitleSizeDropdown.value = PlayerPrefs.GetInt("SubtitleSize", 1);
        subtitleSizeDropdown.Select();
        subtitleSizeDropdown.RefreshShownValue();

        //Sound
        sfxSlider.value = (float)PlayerPrefs.GetInt("SFXVolume", 100) / 100;
        musicSlider.value = (float)PlayerPrefs.GetInt("MusicVolume", 100) / 100;
        voiceSlider.value = (float)PlayerPrefs.GetInt("VoiceVolume", 100) / 100;

        // Display
        fullScreenToggle.isOn = Screen.fullScreen;
        vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 0) == 1;

        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionDropdownValue", 0);
        resolutionDropdown.Select();
        resolutionDropdown.RefreshShownValue();
    }
}

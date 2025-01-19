using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject overviewPage;
    [SerializeField] private GameObject settingsPage;

    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    //[SerializeField] private TextMeshProUGUI resolutionLabel;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    //[SerializeField] private List<ResItem> resolutions = new List<ResItem>();

    public static float sfxVolume { get; private set; }
    public static float musicVolume { get; private set; }

    private FPSController fpsController;
    private GameManager gm;
    private AudioManager am;

    private bool isPaused = false;

    //private int selectedResolution;

    void Start()
    {
        fpsController = FindObjectOfType<FPSController>();
        gm = GameManager.instance;
        am = AudioManager.instance;

        resolutionDropdown.options.Insert(0, new TMP_Dropdown.OptionData(Screen.currentResolution.width + " x " + Screen.currentResolution.height));
        resolutionDropdown.value = 0;
        resolutionDropdown.Select();
        resolutionDropdown.RefreshShownValue();

        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Don't allow pausing when on pc
            if (!isPaused && gm.GetGameState() != GameState.OnPC)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    private void PauseGame()
    {
        //Time.timeScale = 0;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        fpsController.enabled = false;

        OpenOverview();
    }

    public void ResumeGame()
    {
        //Time.timeScale = 1;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        fpsController.enabled = true;

        CloseAllMenus();
    }

    public void OpenOverview()
    {
        overviewPage.SetActive(true);
        settingsPage.SetActive(false);
    }

    public void OpenSettingsMenu()
    {
        overviewPage.SetActive(false);
        settingsPage.SetActive(true);
    }

    private void CloseAllMenus()
    {
        overviewPage.SetActive(false);
        settingsPage.SetActive(false);
    }

    public void CloseGame()
    {
        //Time.timeScale = 1;
        Application.Quit();
    }

    public void OnMusicSliderValueChange(float value)
    {
        am.UpdateMixerValue("Music Volume", value);
    }

    public void OnSfxSliderValueChange(float value)
    {
        am.UpdateMixerValue("SFX Volume", value);
    }

    public void OnVoiceSliderValueChange(float value)
    {
        am.UpdateMixerValue("Voice Volume", value);
    }

    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    /*public void ResLeft()
    {
        selectedResolution--;
        if (selectedResolution < 0)
        {
            selectedResolution = 0;
        }
        UpdateResLabel();
    }

    public void ResRight()
    {
        selectedResolution++;
        if (selectedResolution > resolutions.Count - 1)
        {
            selectedResolution = resolutions.Count - 1;
        }
        UpdateResLabel();
    }*/

    /*public void UpdateResLabel()
    {
        //resolutionLabel.text = resolutions[selectedResolution].horizontal + " x " + resolutions[selectedResolution].vertical;

    }*/

    public void ApplyGraphics()
    {
        QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;

        int horizontalRes = Int32.Parse(resolutionDropdown.options[resolutionDropdown.value].text.Split('x')[0].Trim());
        int verticalRes = Int32.Parse(resolutionDropdown.options[resolutionDropdown.value].text.Split('x')[1].Trim());

        Screen.SetResolution(horizontalRes, verticalRes, fullScreenToggle.isOn);
    }
}

[System.Serializable]
public class ResItem
{
    public int horizontal, vertical;
}
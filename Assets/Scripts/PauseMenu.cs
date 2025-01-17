using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject overviewPage;
    [SerializeField] private GameObject settingsPage;

    public static float sfxVolume { get; private set; }
    public static float musicVolume { get; private set; }

    private FPSController fpsController;
    private GameManager gm;
    private AudioManager am;

    private bool isPaused = false;

    void Start()
    {
        fpsController = FindObjectOfType<FPSController>();
        gm = GameManager.instance;
        am = AudioManager.instance;

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
}

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

    private FPSController fpsController;
    private GameManager gm;
    private AudioManager am;
    private SettingsMenu settings;

    private bool isPaused = false;

    void Start()
    {
        fpsController = FindObjectOfType<FPSController>();
        gm = GameManager.instance;
        am = AudioManager.instance;
        settings = GetComponentInChildren<SettingsMenu>(true);

        settings.ApplyCurrentSettings();

        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape))
        {
            // Don't allow pausing when at computer
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
        am.UpdateMixerValue("SFX Volume", 0.0001f);

        OpenOverview();
    }

    public void ResumeGame()
    {
        //Time.timeScale = 1;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        fpsController.enabled = true;
        am.UpdateMixerValue("SFX Volume", settings.sfxVolume);

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
}
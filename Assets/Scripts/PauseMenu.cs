using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject overviewPage;
    [SerializeField] private GameObject settingsPage;

    private FPSController fpsController;
    private GameManager gm;
    private AudioManager am;
    private SettingsMenu settings;

    private bool isPaused = false;
    private bool pauseMenuOnCooldown = false;

    void Start()
    {
        fpsController = FindFirstObjectByType<FPSController>();
        gm = GameManager.instance;
        am = AudioManager.instance;
        settings = GetComponentInChildren<SettingsMenu>(true);

        settings.AddNativeResolution();
        settings.ApplyCurrentSettings();

        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Don't allow pausing when at computer
            if (!isPaused && gm.GetGameState() == GameState.Playing && !pauseMenuOnCooldown)
            {
                PauseGame();
            }
            else if (gm.GetGameState() == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    private void PauseGame()
    {
        //Time.timeScale = 0;
        isPaused = true;
        gm.SetGameState(GameState.Paused);

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
        gm.SetGameState(GameState.Playing);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        fpsController.enabled = true;
        am.UpdateMixerValue("SFX Volume", settings.sfxVolume);

        CloseAllMenus();
    }

    public IEnumerator StartPauseMenuCooldown()
    {
        pauseMenuOnCooldown = true;
        yield return new WaitForSeconds(0.3f);
        pauseMenuOnCooldown = false;
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
        SceneManager.LoadScene("TitleScene");
    }
}
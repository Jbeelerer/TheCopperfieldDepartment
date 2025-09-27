using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using SaveSystem;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private AudioSource bgm;
    [SerializeField] private GameObject settingsPage;
    [SerializeField] private Button continueButton;

    private Animator anim;
    private AudioSource doorAudio;

    private bool startNewGame = false;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        doorAudio = GetComponent<AudioSource>();

        settingsPage.GetComponent<SettingsMenu>().AddNativeResolution();
        settingsPage.GetComponent<SettingsMenu>().ApplyCurrentSettings();

        continueButton.interactable = SaveManager.instance.GetSaveExists();
    }

    public void PlayStartAnimation(bool isNewGame = false)
    {
        startNewGame = isNewGame;
        doorAudio.Play();
        anim.SetTrigger("StartGame");
    }

    public void StopBGM()
    {
        bgm.Stop();
    }

    // Used in animation event, after start animation has played
    public void StartGame()
    {
        if (startNewGame)
            SaveManager.instance.DeleteSave();

        SceneManager.LoadScene("NewMainScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        settingsPage.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPage.SetActive(false);
    }
}

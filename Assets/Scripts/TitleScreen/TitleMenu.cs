using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private AudioSource bgm;
    [SerializeField] private GameObject settingsPage;

    private Animator anim;
    private AudioSource doorAudio;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        doorAudio = GetComponent<AudioSource>();

        settingsPage.GetComponent<SettingsMenu>().AddNativeResolution();
        settingsPage.GetComponent<SettingsMenu>().ApplyCurrentSettings();
    }

    public void PlayStartAnimation()
    {
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private AudioSource bgm;

    private Animator anim;
    private AudioSource doorAudio;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        doorAudio = GetComponent<AudioSource>();
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
}

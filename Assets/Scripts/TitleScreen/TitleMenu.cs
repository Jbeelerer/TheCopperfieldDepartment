using Cinemachine;
using SaveSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TitleOption
{
    QUIT,
    BACK,
    CONTINUE,
    NEW_GAME,
    NEW_GAME_CONFIRM,
    SETTINGS
}

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private GameObject bgmObject;
    [SerializeField] private GameObject settingsPage;
    [SerializeField] private Button continueButton;
    [SerializeField] private Animator doorAnim;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineVirtualCamera doorCam;
    [SerializeField] private CinemachineVirtualCamera pinboardMainCam;
    [SerializeField] private CinemachineVirtualCamera newGameCam;
    [SerializeField] private CinemachineVirtualCamera settingsCam;
    [SerializeField] private CinemachineVirtualCamera continueCam;
    [SerializeField] private AudioMixer bgmMixer;
    [SerializeField] private AudioClip doorCreakSound;
    [SerializeField] private AudioClip paperRustleSound;
    [SerializeField] private AudioClip wooshSound;

    private Animator anim;
    //private AudioSource audioSource;
    private AudioManager audioManager;
    private List<CinemachineVirtualCamera> cameras;
    private bool startNewGame = false;
    private bool logosShown = false;
    private bool doorOpened = false;
    private GameObject currentSelectedOption;
    private RaycastHit hit;
    private Ray r;

    void Start()
    {
        anim = GetComponent<Animator>();
        audioManager = AudioManager.instance;

        settingsPage.GetComponent<SettingsMenu>().AddNativeResolution();
        settingsPage.GetComponent<SettingsMenu>().ApplyCurrentSettings();
        audioManager.UpdateMixerValue("SFX Volume", settingsPage.GetComponent<SettingsMenu>().sfxVolume);
        var bgmSources = bgmObject.GetComponents<AudioSource>();

        StartCoroutine(PlaySourceAfterTime(bgmSources[1], bgmSources[0].clip.length));

        continueButton.interactable = SaveManager.instance.GetSaveExists();

        cameras = new List<CinemachineVirtualCamera>() { doorCam, pinboardMainCam, newGameCam, settingsCam, continueCam };

        //SetCamPriority(doorCam);
    }

    private IEnumerator PlaySourceAfterTime(AudioSource source, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        source.Play();
    }

    private void Update()
    {
        if (!doorOpened && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Mouse0)))
        {
            if (!logosShown)
            {
                SkipLogos();
            } 
            else
            {
                OpenDoor();
            }
        }

        r = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out hit) && currentSelectedOption != hit.transform.gameObject)
        {
            currentSelectedOption?.GetComponent<TitleMenuOption>().HoverAnimStop();
            currentSelectedOption = null;
            if (hit.transform.GetComponent<TitleMenuOption>())
            {
                hit.transform.GetComponent<TitleMenuOption>().HoverAnimStart();
                audioManager.PlayAudio(paperRustleSound, 0.7f);
                currentSelectedOption = hit.transform.gameObject;
            } 
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && currentSelectedOption)
        {
            SelectOption(currentSelectedOption.transform.GetComponent<TitleMenuOption>().optionType);
        }
    }

    private void SelectOption(TitleOption option)
    {
        cinemachineBrain.m_DefaultBlend.m_Time = 0.3f;
        switch (option)
        {
            case TitleOption.QUIT:
                QuitGame();
                break;
            case TitleOption.SETTINGS:
                OpenSettings();
                break;
            case TitleOption.CONTINUE:
                PlayStartAnimation();
                cinemachineBrain.m_DefaultBlend.m_Time = 2f;
                SetCamPriority(continueCam);
                break;
            case TitleOption.NEW_GAME:
                SetCamPriority(newGameCam);
                break;
            case TitleOption.NEW_GAME_CONFIRM:
                startNewGame = true;
                PlayStartAnimation();
                break;
            case TitleOption.BACK:
                SetCamPriority(pinboardMainCam);
                break;
        }
    }

    private void SetCamPriority(CinemachineVirtualCamera cam)
    {
        cameras.ForEach(c => c.Priority = 0);
        cam.Priority = 1;

        audioManager.PlayAudio(wooshSound, 0.7f);
    }

    public void PlayStartAnimation()
    {
        anim.SetTrigger("StartGame");
        StartCoroutine(MusicFadeOut());
    }

    private IEnumerator LowPassFadeOut()
    {
        float fadeTime = 3f;
        float t = fadeTime;
        float lowpassStartValue = 290f;
        float lowpassEndValue = 22000f;
        float lowpassDifference = lowpassEndValue - lowpassStartValue;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
            bgmMixer.SetFloat("LowpassCutoff", lowpassEndValue - (lowpassDifference * (t / fadeTime)));
        }
        yield break;
    }

    private IEnumerator MusicFadeOut()
    {
        float fadeTime = 2f;
        float t = fadeTime;
        float musicVolume = settingsPage.GetComponent<SettingsMenu>().musicVolume;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
            audioManager.UpdateMixerValue("Title Music Volume", musicVolume * (t / fadeTime));
        }
        yield break;
    }

    private void OpenDoor()
    {
        doorAnim.Play("DoorOpen");
        StartCoroutine(LowPassFadeOut());
        SetCamPriority(pinboardMainCam);
        audioManager.PlayAudio(doorCreakSound);
        doorOpened = true;
    }

    public void SkipLogos()
    {
        anim.Play("TitleFadeIn");
        logosShown = true;
    }

    // Used in animation event, after start animation has played
    public void StartGame()
    {
        if (startNewGame)
            SaveManager.instance.DeleteSave();

        SceneManager.LoadScene("NewMainScene");
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void OpenSettings()
    {
        SetCamPriority(settingsCam);
    }

    public void CloseSettings()
    {
        SetCamPriority(pinboardMainCam);
        audioManager.UpdateMixerValue("SFX Volume", settingsPage.GetComponent<SettingsMenu>().sfxVolume);
    }
}

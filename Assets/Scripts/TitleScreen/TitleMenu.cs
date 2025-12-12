using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SaveSystem;
using UnityEngine.UI;
using Cinemachine;

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
    [SerializeField] private AudioSource bgm;
    [SerializeField] private GameObject settingsPage;
    [SerializeField] private Button continueButton;
    [SerializeField] private Animator doorAnim;
    [SerializeField] private CinemachineVirtualCamera doorCam;
    [SerializeField] private CinemachineVirtualCamera pinboardMainCam;
    [SerializeField] private CinemachineVirtualCamera newGameCam;
    [SerializeField] private CinemachineVirtualCamera settingsCam;
    [SerializeField] private CinemachineVirtualCamera continueCam;

    private Animator anim;
    private AudioSource doorAudio;
    private List<CinemachineVirtualCamera> cameras;
    private bool startNewGame = false;
    private bool logosShown = false;
    private GameObject currentSelectedOption;
    private RaycastHit hit;
    private Ray r;

    void Start()
    {
        anim = GetComponent<Animator>();
        doorAudio = GetComponent<AudioSource>();

        settingsPage.GetComponent<SettingsMenu>().AddNativeResolution();
        settingsPage.GetComponent<SettingsMenu>().ApplyCurrentSettings();

        continueButton.interactable = SaveManager.instance.GetSaveExists();

        cameras = new List<CinemachineVirtualCamera>() { doorCam, pinboardMainCam, newGameCam, settingsCam, continueCam };

        SetCamPriority(doorCam);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (!logosShown)
            {
                SkipLogos();
            } 
            else
            {
                doorAnim.Play("DoorOpen");
                SetCamPriority(pinboardMainCam);
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
                currentSelectedOption = hit.transform.gameObject;
            } 
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && currentSelectedOption)
        {
            SelectOption(currentSelectedOption.transform.GetComponent<TitleMenuOption>().optionType);
            print("option selected: " + currentSelectedOption.transform.GetComponent<TitleMenuOption>().optionType);
        }
    }

    private void SelectOption(TitleOption option)
    {
        switch (option)
        {
            case TitleOption.QUIT:
                break;
            case TitleOption.SETTINGS:
                break;
            case TitleOption.CONTINUE:
                break;
            case TitleOption.NEW_GAME:
                break;
            case TitleOption.NEW_GAME_CONFIRM:
                break;
            case TitleOption.BACK:
                break;
        }
    }

    private void SetCamPriority(CinemachineVirtualCamera cam)
    {
        cameras.ForEach(c => c.Priority = 0);
        cam.Priority = 1;
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

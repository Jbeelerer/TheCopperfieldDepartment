using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class TimedSubtitle
{
    public float duration;
    public string text;
}
[System.Serializable]
public class TimedSubtitles
{
    public TimedSubtitle[] intro;
    public TimedSubtitle[] firstDayFeedbackPositive;
    public TimedSubtitle[] firstDayFeedbackNegative;
}

public class Narration : MonoBehaviour
{

    [SerializeField] private AudioClip notLeavingVoice;
    [SerializeField] private AudioClip positiveFeedbackVoice;
    [SerializeField] private AudioClip negativeFeedbackVoice;
    [SerializeField] private AudioClip deletePostVoice;
    [SerializeField] private AudioClip suspectFoundVoice;
    [SerializeField] private string notLeavingText;
    [SerializeField] private string positiveFeedbackText;
    [SerializeField] private string negativeFeedbackText;
    [SerializeField] private string deletePostText;
    [SerializeField] private string suspectFoundText;
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip firstDayFeedbackPositiveClip;
    [SerializeField] private AudioClip firstDayFeedbackNegativeClip;

    private TimedSubtitles timedSubtitles;
    private TextMeshProUGUI subtitleText;

    private AudioSource audioSource;

    private GameManager gm;

    private AudioManager am;

    private GameObject blackScreen;
    [SerializeField] private TextAsset jsonFile;

    [SerializeField] private AudioClip phonePickup;
    [SerializeField] private AudioClip phoneHangup;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        am = AudioManager.instance;
        gm.SetNarration(this);
        audioSource = GetComponent<AudioSource>();
        subtitleText = GameObject.Find("Subtitle").GetComponent<TextMeshProUGUI>();

        blackScreen = GameObject.Find("BlackScreen");
        blackScreen.SetActive(false);


        timedSubtitles = JsonUtility.FromJson<TimedSubtitles>(jsonFile.text);


        if (gm.GetDay() == 1)
        {
            PlaySequence("intro");
        }
    }

    public void PlaySequence(string sequence)
    {
        switch (sequence)
        {
            case "firstDayFeedbackPositive":
                StartCoroutine(PlaySequence(timedSubtitles.firstDayFeedbackPositive, firstDayFeedbackPositiveClip));
                break;
            case "firstDayFeedbackNegative":
                StartCoroutine(PlaySequence(timedSubtitles.firstDayFeedbackNegative, firstDayFeedbackNegativeClip));
                break;
            case "intro":
                StartCoroutine(PlaySequence(timedSubtitles.intro, introClip));
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.GetIfDevMode())
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                StopAllCoroutines();

                gm.SetGameState(GameState.Playing);
                subtitleText.text = "";
                subtitleText.fontSize -= 10;
                blackScreen.SetActive(false);
            }
        }

    }
    public void Say(string text)
    {
        switch (text)
        {
            case "notLeaving":
                audioSource.clip = notLeavingVoice;
                subtitleText.text = notLeavingText;
                break;
            case "positiveFeedback":
                audioSource.clip = positiveFeedbackVoice;
                subtitleText.text = positiveFeedbackText;
                break;
            case "negativeFeedback":
                audioSource.clip = negativeFeedbackVoice;
                subtitleText.text = negativeFeedbackText;
                break;
            case "deletePost":
                audioSource.clip = deletePostVoice;
                subtitleText.text = deletePostText;
                break;
            case "suspectFound":
                audioSource.clip = suspectFoundVoice;
                subtitleText.text = suspectFoundText;
                break;
        }
        StartCoroutine(DisableSubtitle(audioSource.clip.length + 1));
        audioSource.Play();
    }

    public IEnumerator playFastSound(AudioClip audioClip, float pitch)
    {
        audioSource.clip = audioClip;
        audioSource.pitch = pitch;
        audioSource.Play();
        yield return new WaitForSeconds(audioClip.length * pitch);
        audioSource.pitch = 1;
    }
    private IEnumerator DisableSubtitle(float time)
    {
        yield return new WaitForSeconds(time);
        subtitleText.text = "";
    }

    public IEnumerator PlaySequence(TimedSubtitle[] content, AudioClip clip)
    {
        gm.SetGameState(GameState.Frozen);
        blackScreen.SetActive(true);
        subtitleText.text = "";
        am.PlayAudio(phonePickup);
        yield return new WaitForSeconds(phonePickup.length);
        audioSource.clip = clip;
        audioSource.Play();
        foreach (TimedSubtitle entry in content)
        {
            subtitleText.text = entry.text;
            yield return new WaitForSeconds(entry.duration);

        }
        am.PlayAudio(phoneHangup);
        yield return new WaitForSeconds(phonePickup.length);
        gm.SetGameState(GameState.Playing);
        subtitleText.text = "";
        blackScreen.SetActive(false);
        if (gm.GetDay() != 1)
            gm.NextDaySequence();
    }
}

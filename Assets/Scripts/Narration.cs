using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public TimedSubtitle[] phoneCallIntro;
    public TimedSubtitle[] firstDayFeedbackPositive;
    public TimedSubtitle[] firstDayFeedbackNegative;
}
[System.Serializable]
public class ShortSubtitles
{
    public TimedSubtitle notLeaving;
    public TimedSubtitle positiveFeedback;
    public TimedSubtitle negativeFeedback;
    public TimedSubtitle deletePost;
    public TimedSubtitle suspectFound;
}

public class Narration : MonoBehaviour
{

    [SerializeField] private AudioClip notLeavingVoice;
    [SerializeField] private AudioClip positiveFeedbackVoice;
    [SerializeField] private AudioClip negativeFeedbackVoice;
    [SerializeField] private AudioClip deletePostVoice;
    [SerializeField] private AudioClip suspectFoundVoice;
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip firstDayFeedbackPositiveClip;
    [SerializeField] private AudioClip firstDayFeedbackNegativeClip;

    [SerializeField] private AudioClip phoneCallIntroClip;

    private TimedSubtitles timedSubtitles;
    private TextMeshProUGUI subtitleText;

    private AudioSource audioSource;

    private GameManager gm;

    private AudioManager am;

    private GameObject blackScreen;
    [SerializeField] private TextAsset jsonFile;

    [SerializeField] private TextAsset shortSubtitlesJsonFile;
    ShortSubtitles shortSubtitles;

    [SerializeField] private AudioClip phonePickup;
    [SerializeField] private AudioClip phoneHangup;

    Quaternion[] rotations;

    private bool sequencePlaying = false;
    private bool skip = false;

    private Animator textAnimator;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        am = AudioManager.instance;
        gm.SetNarration(this);
        audioSource = GetComponent<AudioSource>();
        subtitleText = GameObject.Find("Subtitle").GetComponent<TextMeshProUGUI>();
        textAnimator = subtitleText.GetComponent<Animator>();

        blackScreen = GameObject.Find("BlackScreen");
        blackScreen.SetActive(false);



        timedSubtitles = JsonUtility.FromJson<TimedSubtitles>(jsonFile.text);
        shortSubtitles = JsonUtility.FromJson<ShortSubtitles>(shortSubtitlesJsonFile.text);

        if (gm.GetDay() == 1)
        {
            PlaySequence("intro");
        }
    }

    public void PlaySequence(string sequence, Quaternion[] rotation = null)
    {
        rotations = rotation;
        blackScreen.GetComponent<Image>().color = rotation != null ? new Color(0, 0, 0, 0f) : new Color(0, 0, 0, 1f);
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
            case "phoneCallIntro":
                StartCoroutine(PlaySequence(timedSubtitles.phoneCallIntro, phoneCallIntroClip));
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
                audioSource.Stop();
                gm.SetGameState(GameState.Playing);
                subtitleText.text = "";
                blackScreen.SetActive(false);
            }
        }

        if (Input.GetMouseButtonDown(0) && sequencePlaying && !skip)
        {
            skip = true;
        }
    }
    public void Say(string text)
    {
        float duration = 0;
        switch (text)
        {
            case "notLeaving":
                audioSource.clip = notLeavingVoice;
                subtitleText.text = shortSubtitles.notLeaving.text;
                duration = shortSubtitles.notLeaving.duration;
                break;
            case "positiveFeedback":
                audioSource.clip = positiveFeedbackVoice;
                subtitleText.text = shortSubtitles.positiveFeedback.text;
                duration = shortSubtitles.positiveFeedback.duration;
                break;
            case "negativeFeedback":
                audioSource.clip = negativeFeedbackVoice;
                subtitleText.text = shortSubtitles.negativeFeedback.text;
                duration = shortSubtitles.negativeFeedback.duration;
                break;
            case "deletePost":
                audioSource.clip = deletePostVoice;
                subtitleText.text = shortSubtitles.deletePost.text;
                duration = shortSubtitles.deletePost.duration;
                break;
            case "suspectFound":
                audioSource.clip = suspectFoundVoice;
                subtitleText.text = shortSubtitles.suspectFound.text;
                duration = shortSubtitles.suspectFound.duration;
                break;
        }
        StartCoroutine(DisableSubtitle(duration));
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

        FPSController player = GameObject.Find("Player").GetComponent<FPSController>();
        Quaternion startRotation = player.transform.rotation;
        gm.SetGameState(GameState.Frozen);
        blackScreen.SetActive(true);

        subtitleText.text = "";

        am.PlayAudio(phonePickup);
        yield return new WaitForSeconds(phonePickup.length);

        audioSource.time = 0;
        int talkIndex = 0;
        float totalTime = 0;
        sequencePlaying = true;

        audioSource.clip = clip;
        audioSource.Play();

        foreach (TimedSubtitle entry in content)
        {
            if (rotations != null)
            {
                //Camera.main.transform.rotation = rotations[i];  
                player.ResetCameraRotation(rotations[talkIndex]);
                talkIndex++;
            }
            subtitleText.text = entry.text;
            float time = 0;
            while (entry.duration >= time)
            {
                yield return new WaitForSeconds(0.1f);
                time += 0.1f;
                if (skip)
                {
                    if (entry.duration + totalTime < audioSource.clip.length)
                    {
                        print(totalTime + entry.duration);
                        audioSource.Stop();
                        audioSource.time = totalTime + entry.duration;
                        audioSource.Play();
                    }
                    textAnimator.Play("skip");
                    skip = false;
                    break;
                }
            }
            totalTime += entry.duration;
        }
        player.ResetCameraRotation(startRotation);


        sequencePlaying = false;
        am.PlayAudio(phoneHangup);
        yield return new WaitForSeconds(phonePickup.length);
        gm.SetGameState(GameState.Playing);
        subtitleText.text = "";
        blackScreen.SetActive(false);
        if (gm.GetDay() != 1)
            gm.NextDaySequence();
    }

    private void LoadShortSubtitles()
    {
        // ShortSubtitles shortSubtitles = JsonUtility.FromJson<ShortSubtitles>(shortSubtitlesJsonFile.text);
        //notLeavingText = shortSubtitles.notLeaving.text;
        //positiveFeedbackText = shortSubtitles.positiveFeedback.text;
    }
}

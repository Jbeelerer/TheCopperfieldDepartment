using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Requirement
{
    None,
    WalkToBoard,
    LookAtPostIt,
    MovePostIt,
    DeletePostIt,
    FindSuspect,
    ConnectPostIT,
    ConnectPostITContradiction,
    CutThread,
    Pen,
    AnnotateCircle,
    AnnotateCross,
}

[System.Serializable]
public class TimedSubtitle
{
    public float duration;
    public string text;
    public Requirement requirement = Requirement.None;
}
[System.Serializable]
public class TimedSubtitles
{
    public TimedSubtitle[] intro;
    public TimedSubtitle[] phoneReminderPostNotAdded;
    public TimedSubtitle[] phoneReminderPersonNotAdded;
    public TimedSubtitle[] phoneReminderNothingAdded;
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
    public TimedSubtitle phoneNotWorking;
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
    [SerializeField] private AudioClip phoneNotWorkingClip;

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

    private Requirement requirementToBeMet = Requirement.None;
    private bool requirementMet = false;

    private Quaternion[] rotations;

    private bool sequencePlaying = false;
    private bool skip = false;
    private Animator textAnimator;

    Quaternion startRotation;

    private bool sequenceHadRequirement = false;
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
        blackScreen.GetComponent<Image>().color = rotation != null ? new Color(0, 0, 0, 0f) : new Color(0, 0, 0, 1f);
        StopAllCoroutines();
        switch (sequence)
        {
            case "firstDayFeedbackPositive":
                StartCoroutine(PlaySequence(timedSubtitles.firstDayFeedbackPositive, firstDayFeedbackPositiveClip));
                break;
            case "firstDayFeedbackNegative":
                StartCoroutine(PlaySequence(timedSubtitles.firstDayFeedbackNegative, firstDayFeedbackNegativeClip));
                break;
            case "intro":
                StartCoroutine(PlaySequence(timedSubtitles.intro, introClip, false));
                break;
            case "phoneCallIntro":
                StartCoroutine(PlaySequence(timedSubtitles.phoneCallIntro, phoneCallIntroClip, false));
                break;
            case "phoneReminderPostNotAdded":
                StartCoroutine(PlaySequence(timedSubtitles.phoneReminderPostNotAdded, phoneCallIntroClip, false));
                break;
            case "phoneReminderNothingAdded":
                StartCoroutine(PlaySequence(timedSubtitles.phoneReminderNothingAdded, phoneCallIntroClip, false));
                break;
        }
    }

    public bool HasRequirement()
    {
        return requirementToBeMet != Requirement.None;
    }
    public bool CheckIfRequirementMet(Requirement requirement, bool lastStep = true)
    {
        if (requirementToBeMet == requirement)
        {
            if (lastStep)
            {
                requirementMet = true;
                requirementToBeMet = Requirement.None;
            }
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.GetIfDevMode())
        {
            // remove completly skipping for now
            /*  if (Input.GetKeyDown(KeyCode.X) && sequencePlaying)
              {
                  StopAllCoroutines();
                  audioSource.Stop();
                  gm.SetGameState(GameState.Playing);
                  subtitleText.text = "";
                  blackScreen.SetActive(false);
                  if (rotations != null)
                  {
                      GameObject.Find("Player").GetComponent<FPSController>().ResetCameraRotation(startRotation);
                  }
              }*/
        }

        if (Input.GetMouseButtonDown(0) && sequencePlaying && !skip && !sequenceHadRequirement)
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
            case "phoneNotWorking":
                audioSource.clip = phoneNotWorkingClip;
                subtitleText.text = shortSubtitles.phoneNotWorking.text;
                duration = shortSubtitles.phoneNotWorking.duration;
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

    public IEnumerator PlaySequence(TimedSubtitle[] content, AudioClip clip, bool playNextDayAnimation = true)
    {

        FPSController player = GameObject.Find("Player").GetComponent<FPSController>();
        startRotation = player.transform.rotation;
        gm.SetGameState(GameState.Frozen);
        blackScreen.SetActive(true);

        subtitleText.text = "";

        am.PlayAudio(phonePickup);
        yield return new WaitForSeconds(0.05f);
        int talkIndex = 0;
        float totalTime = 0;
        sequencePlaying = true;

        audioSource.time = 0;
        audioSource.clip = clip;
        audioSource.Play();

        foreach (TimedSubtitle entry in content)
        {
            totalTime += entry.duration;
            requirementMet = false;
            print(entry.requirement);
            print(entry.text);
            do
            {
                if (rotations != null)
                {
                    if (rotations.Length > talkIndex)
                    {
                        //Camera.main.transform.rotation = rotations[i];  
                        player.ResetCameraRotation(rotations[talkIndex]);
                        talkIndex++;
                    }
                    else if (gm.GetGameState() == GameState.Frozen)
                    {
                        player.ResetCameraRotation(startRotation);
                    }
                }
                subtitleText.text = entry.text;
                float time = 0;
                while (entry.duration >= time)
                {
                    yield return new WaitForSeconds(0.1f);
                    time += 0.1f;
                    if (skip)
                    {
                        skip = false;
                        if (entry.requirement != Requirement.None)
                        {
                            continue;
                        }
                        if (entry.duration + totalTime < audioSource.clip.length)
                        {
                            audioSource.Stop();
                            audioSource.time = totalTime;
                            audioSource.Play();
                        }
                        textAnimator.Play("skip");
                        break;
                    }
                }
                if (entry.requirement != Requirement.None)
                {
                    sequenceHadRequirement = true;
                    audioSource.Stop();
                    requirementToBeMet = entry.requirement;
                    if (gm.GetGameState() != GameState.Playing)
                        gm.SetGameState(GameState.Playing);
                    // TODO: Multithreading might be an option here
                    for (int i = 0; i < 50; i++)
                    {
                        yield return new WaitForSeconds(0.1f);
                        if (requirementMet)
                        {
                            break;
                        }
                    }
                    if (!requirementMet && totalTime - entry.duration < audioSource.clip.length)
                    {
                        audioSource.time = totalTime - entry.duration;
                        audioSource.Play();
                    }
                }
                // yield return new WaitForSeconds(0.1f);

            } while (entry.requirement != Requirement.None && !requirementMet);
            if (requirementMet && totalTime < audioSource.clip.length)
            {
                audioSource.time = totalTime;
                audioSource.Play();
            }
        }
        if (gm.GetGameState() == GameState.Frozen)
        {
            player.ResetCameraRotation(startRotation);
        }
        sequenceHadRequirement = false;
        sequencePlaying = false;
        am.PlayAudio(phoneHangup);
        yield return new WaitForSeconds(phonePickup.length);
        gm.SetGameState(GameState.Playing);
        subtitleText.text = "";

        if (clip == introClip)
        {
            GameObject ow = GameObject.Find("OutsideWorld");
            if (ow != null)
            {
                ow.GetComponent<Animator>().SetTrigger("NewDay");
            }
        }

        blackScreen.SetActive(false);
        if (playNextDayAnimation)
            gm.NextDaySequence();
    }

    private void LoadShortSubtitles()
    {
        // ShortSubtitles shortSubtitles = JsonUtility.FromJson<ShortSubtitles>(shortSubtitlesJsonFile.text);
        //notLeavingText = shortSubtitles.notLeaving.text;
        //positiveFeedbackText = shortSubtitles.positiveFeedback.text;
    }
}

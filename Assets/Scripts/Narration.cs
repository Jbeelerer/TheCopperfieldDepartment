using System.Collections;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
    public TimedSubtitle[] exit;
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
    [SerializeField] private AudioClip exit;

    [SerializeField] private AudioClip phoneCallIntroClip;

    [SerializeField] private AudioClip phoneCallNothingAddedClip;

    [SerializeField] private AudioClip phoneCallNoPostClip;

    [SerializeField] private AudioClip phoneCallNoPersonClip;

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
    private bool interactionAllowed = false;

    private Quaternion[] rotations;

    private bool sequencePlaying = false;
    private bool skip = false;
    private Animator textAnimator;

    private bool isTalking = false;
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
        rotations = rotation;
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
            case "exit":
                StartCoroutine(PlaySequence(timedSubtitles.exit, exit, false));
                break;
            case "intro":
                StartCoroutine(PlaySequence(timedSubtitles.intro, introClip, false));
                break;
            case "phoneCallIntro":
                StartCoroutine(PlaySequence(timedSubtitles.phoneCallIntro, phoneCallIntroClip, false));
                break;
            case "phoneReminderPostNotAdded":
                StartCoroutine(PlaySequence(timedSubtitles.phoneReminderPostNotAdded, phoneCallNoPostClip, false));
                break;
            case "phoneReminderPersonNotAdded":
                StartCoroutine(PlaySequence(timedSubtitles.phoneReminderPersonNotAdded, phoneCallNoPersonClip, false));
                break;
            case "phoneReminderNothingAdded":
                StartCoroutine(PlaySequence(timedSubtitles.phoneReminderNothingAdded, phoneCallNothingAddedClip, false));
                break;
        }
    }

    public bool HasRequirement()
    {
        return sequenceHadRequirement || requirementToBeMet != Requirement.None;
    }
    public bool GetIfRequirementMet()
    {
        return requirementMet;
    }
    public bool GetIfInteractionAllowed()
    {
        if (interactionAllowed)// && !isTalking)
        {
            bool temp = interactionAllowed;
            interactionAllowed = false;
            return temp;
        }
        return false;
    }
    public void CheckIfRequirementMet(Requirement requirement, bool lastStep = true)
    {
        if (requirementToBeMet == requirement)
        {
            if (lastStep)
            {
                requirementMet = true;
                requirementToBeMet = Requirement.None;
            }
            interactionAllowed = true;
        }
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
        Radio radio = FindObjectOfType<Radio>();
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
            interactionAllowed = false;
            skip = false;
            do
            {
                radio.ChangeVolumeTemp(0.2f);
                if (rotations != null)
                {
                    if (rotations.Length > talkIndex)
                    {
                        player.ResetCameraRotation(rotations[talkIndex]);
                        talkIndex++;
                    }
                    else if (gm.GetGameState() == GameState.Frozen)
                    {
                        player.ResetCameraRotation(startRotation);
                    }
                }
                //set requirement if there is one
                if (entry.requirement != Requirement.None)
                {
                    sequenceHadRequirement = true;
                    requirementMet = false;
                    interactionAllowed = false;
                    requirementToBeMet = entry.requirement;
                }
                else if (sequenceHadRequirement)
                {
                    requirementToBeMet = Requirement.None;
                    requirementMet = false;
                    interactionAllowed = false;
                }
                subtitleText.text = entry.text;
                isTalking = true;
                float startTime = Time.time;
                while (!skip && (Time.time - startTime < entry.duration))
                {
                    yield return null;
                }
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
                }

                isTalking = false;
                if (entry.requirement != Requirement.None)
                {
                    radio.ResetVolume();
                    audioSource.Stop();
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
        FindAnyObjectByType<Phone>().ResetPhone();
        blackScreen.SetActive(false);
        if (playNextDayAnimation)
        {
            FindObjectOfType<Radio>().PauseRadio();
            gm.NextDaySequence();
        }
    }

    public bool CancelSequence()
    {
        if (sequencePlaying)
        {
            StopAllCoroutines();
            audioSource.Stop();
            gm.SetGameState(GameState.Playing);
            subtitleText.text = "";
            blackScreen.SetActive(false);
            sequenceHadRequirement = false;
            sequencePlaying = false;
            requirementMet = false;
            requirementToBeMet = Requirement.None;
            FindAnyObjectByType<Phone>().ResetPhone();
            am.PlayAudio(phoneHangup);
            if (rotations != null)
            {
                GameObject.Find("Player").GetComponent<FPSController>().ResetCameraRotation(startRotation);
            }
            return true;
        }
        return false;
    }

    private void LoadShortSubtitles()
    {
        // ShortSubtitles shortSubtitles = JsonUtility.FromJson<ShortSubtitles>(shortSubtitlesJsonFile.text);
        //notLeavingText = shortSubtitles.notLeaving.text;
        //positiveFeedbackText = shortSubtitles.positiveFeedback.text;
    }
}

using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
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
    public string focus;
}
[System.Serializable]
public class SlideWithAnim
{
    public int voideLineIndex;
    public String animation;
    public Sprite slide;
}
[System.Serializable]
public class Slides
{
    public string slideName;
    public String[] animation;
    public Sprite[] slide;
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
    public TimedSubtitle blocked;
    public TimedSubtitle phoneCallIntro_replay;
    public TimedSubtitle phoneReminderNothingAdded_replay;
    public TimedSubtitle phoneReminderPersonNotAdded_replay;
    public TimedSubtitle phoneReminderPostNotAdded_replay;
    public TimedSubtitle exit_replay;
}

public class Narration : MonoBehaviour
{
    [SerializeField] private Slides[] allSlides;
    [SerializeField] private SlideWithAnim[] allAnimations;
    [SerializeField] private GameObject slideDisplayer;
    [SerializeField] private AudioMixerGroup voiceMixerGroup;

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
    [SerializeField] private Animator animator;

    private Requirement requirementToBeMet = Requirement.None;
    private bool requirementMet = false;
    private bool interactionAllowed = false;

    private Quaternion[] rotations;

    private bool sequencePlaying = false;
    private bool skip = false;
    private Animator textAnimator;

    private bool isTalking = false;
    private Quaternion startRotation;
    private bool animationPlaying = false;

    private bool sequenceHadRequirement = false;

    private IEnumerator currentCall;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        am = AudioManager.instance;
        gm.SetNarration(this);
        audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = voiceMixerGroup;
        subtitleText = GameObject.Find("Subtitle").GetComponent<TextMeshProUGUI>();
        textAnimator = subtitleText.GetComponent<Animator>();

        blackScreen = GameObject.Find("BlackScreen");

        timedSubtitles = JsonUtility.FromJson<TimedSubtitles>(jsonFile.text);
        shortSubtitles = JsonUtility.FromJson<ShortSubtitles>(shortSubtitlesJsonFile.text);

        StartCoroutine(CheckIntroSequence());
    }
    public IEnumerator CheckIntroSequence()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (gm.GetDay() == 1)
        {
            gm.PinboardBlocked = true;
            PlaySequence("intro");
        }
        else
        {
        if(blackScreen.transform.parent.GetComponent<Animator>().GetBool("onPhone")){
        blackScreen.transform.parent.GetComponent<Animator>().SetBool("onPhone", false);}
        else{
         blackScreen.transform.parent.GetComponent<Animator>().SetBool("on", false);
        }
        }
    }

    public void PlaySequence(string sequence, Quaternion[] rotation = null)
    {
        rotations = rotation;
        //blackScreen.GetComponent<Image>().color = rotation != null ? new Color(0, 0, 0, 0f) : new Color(0, 0, 0, 1f);
        
        if(rotation != null){
        blackScreen.transform.parent.GetComponent<Animator>().SetBool("onPhone", true);
        }
        else
        {
        blackScreen.transform.parent.GetComponent<Animator>().SetBool("on", true);
        }
        StopAllCoroutines();
        // return if a coroutine is already running
        switch (sequence)
        {
            case "firstDayFeedbackPositive":
                currentCall = PlaySequence(timedSubtitles.firstDayFeedbackPositive, firstDayFeedbackPositiveClip);
                break;
            case "firstDayFeedbackNegative":
                currentCall = PlaySequence(timedSubtitles.firstDayFeedbackNegative, firstDayFeedbackNegativeClip);
                break;
            case "exit":
                currentCall = PlaySequence(timedSubtitles.exit, exit, false);
                break;
            case "intro":
                currentCall = PlaySequence(timedSubtitles.intro, introClip, false, false, true);
                break;
            case "phoneCallIntro":
                gm.PinboardBlocked = false;
                currentCall = PlaySequence(timedSubtitles.phoneCallIntro, phoneCallIntroClip, false);
                break;
            case "phoneReminderPostNotAdded":
                currentCall = PlaySequence(timedSubtitles.phoneReminderPostNotAdded, phoneCallNoPostClip, false);
                break;
            case "phoneReminderPersonNotAdded":
                currentCall = PlaySequence(timedSubtitles.phoneReminderPersonNotAdded, phoneCallNoPersonClip, false);
                break;
            case "phoneReminderNothingAdded":
                currentCall = PlaySequence(timedSubtitles.phoneReminderNothingAdded, phoneCallNothingAddedClip, false);
                break;
        }
        StartCoroutine(currentCall);
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
            case "blocked":
                audioSource.clip = null;
                subtitleText.text = shortSubtitles.blocked.text;
                duration = shortSubtitles.phoneNotWorking.duration;
                break;
            case "phoneCallIntro_replay":
                audioSource.clip = null;
                subtitleText.text = shortSubtitles.phoneCallIntro_replay.text;
                duration = shortSubtitles.phoneCallIntro_replay.duration;
                break;
            case "phoneReminderNothingAdded_replay":
                audioSource.clip = null;
                subtitleText.text = shortSubtitles.phoneReminderNothingAdded_replay.text;
                duration = shortSubtitles.phoneReminderNothingAdded_replay.duration;
                break;
            case "phoneReminderPersonNotAdded_replay":
                audioSource.clip = null;
                subtitleText.text = shortSubtitles.phoneReminderPersonNotAdded_replay.text;
                duration = shortSubtitles.phoneReminderPersonNotAdded_replay.duration;
                break;
            case "phoneReminderPostNotAdded_replay":
                audioSource.clip = null;
                subtitleText.text = shortSubtitles.phoneReminderPostNotAdded_replay.text;
                duration = shortSubtitles.phoneReminderPostNotAdded_replay.duration;
                break;
            case "exit_replay":
                audioSource.clip = null;
                subtitleText.text = shortSubtitles.exit_replay.text;
                duration = shortSubtitles.exit_replay.duration;
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
    public IEnumerator AddToSequenceQue(TimedSubtitle[] content, AudioClip clip, bool playNextDayAnimation = true)
    {
        if (currentCall != null)
        {
            yield return currentCall;
        }
        currentCall = PlaySequence(content, clip, playNextDayAnimation);
    }

    public IEnumerator PlaySequence(TimedSubtitle[] content, AudioClip clip, bool playNextDayAnimation = true, bool autoplay = true, bool slides = false)
    {
        AudioManager.instance.StartSequenceMix();
        FPSController player = GameObject.Find("Player").GetComponent<FPSController>();
        Radio radio = FindObjectOfType<Radio>();
        startRotation = player.transform.rotation;
        gm.SetGameState(GameState.Frozen);
        subtitleText.text = "";
        am.PlayAudio(phonePickup);
        yield return new WaitForSeconds(0.05f);
        int talkIndex = 0;
        float totalTime = 0;
        sequencePlaying = true;

        audioSource.time = 0;
        audioSource.clip = clip;
        audioSource.Play();
        bool lookedAway = false;
        int slideCounter = 0;

        slideDisplayer.SetActive(slides);
        slideDisplayer.transform.parent.Find("MousePrompt").gameObject.SetActive(false);


        foreach (TimedSubtitle entry in content)
        {
            totalTime += entry.duration;
            requirementMet = false;
            interactionAllowed = false;
            skip = false;
            do
            {
                radio.ChangeVolumeTemp(0.2f);
                /*if (rotations != null)
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
                }*/
                if (entry.focus != null)
                {
                    lookedAway = true;
                    Transform f = GameObject.Find(entry.focus).transform;
                    Quaternion TempRo = Quaternion.LookRotation(f.position - player.transform.position);
                    player.ResetCameraRotation(Quaternion.Euler(-20, TempRo.eulerAngles.y, 0));
                }
                else if (lookedAway)
                {
                    player.ResetCameraRotation(startRotation);
                    lookedAway = false;
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
                Coroutine pause = null;
                if (!autoplay)
                {
                    pause = StartCoroutine(pauseSequence(entry.duration));
                    while (!skip)
                    {
                        yield return null;
                    }
                }
                else
                {
                    while ((!skip) && Time.time - startTime < entry.duration)
                    {
                        yield return null;
                    }
                }

                if (skip)
                {
                    if (!autoplay)
                    {
                        StopCoroutine(pause);
                    }
                    if (slides)
                    {

                        
                        slideDisplayer.transform.parent.Find("MousePrompt").gameObject.SetActive(false);
                        slideCounter++;
                       
                            //check if allAnimation contains a voicelineindex which is slideCounter and if yes get that element
                            var slide = allAnimations.FirstOrDefault(x => x.voideLineIndex == slideCounter);
                            if (slide != null)
                            {
                                animator.Play(slide.animation);
                                if (slide.animation == "switchSlides")
                                {
                                    // todo: mouse effect handling!!!
                                    animator.GetComponent<AudioSource>().Play();
                                    animationPlaying = true;
                                    StartCoroutine(delayNewSlide(1, slide.slide));
                                }
                                else
                                {
                                    print("not switchSlide -" + slide.animation);
                                    slideDisplayer.transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = slide.slide;
                                }
                            }
                           
                           // slideDisplayer.transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = allSlides[0].slide[slideCounter];
                        
                    }
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
                    else
                    {
                        audioSource.Stop();
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
        if (gm.GetGameState() == GameState.Frozen && firstDayFeedbackNegativeClip != clip && firstDayFeedbackPositiveClip != clip)
        {
            print("Resetting camera rotation");
            player.ResetCameraRotation(startRotation);
        }
        sequenceHadRequirement = false;
        sequencePlaying = false;
        am.PlayAudio(phoneHangup);
        yield return new WaitForSeconds(phonePickup.length);
        gm.SetGameState(GameState.Playing);
        subtitleText.text = "";
        slideDisplayer.SetActive(false);

        if (clip == introClip)
        {
            GameObject ow = GameObject.Find("OutsideWorld");
            if (ow != null)
            {
                ow.GetComponent<Animator>().SetTrigger("NewDay");
            }
        }
        Phone phone = FindObjectOfType<Phone>();
        if (!phone.GetIsRinging())
        {
            phone.ResetPhone();
        }
        
        if(blackScreen.transform.parent.GetComponent<Animator>().GetBool("onPhone")){
            blackScreen.transform.parent.GetComponent<Animator>().SetBool("onPhone", false);}
        else{
            blackScreen.transform.parent.GetComponent<Animator>().SetBool("on", false);
        }
        if (playNextDayAnimation)
        {
            FindObjectOfType<Radio>().PauseRadio();
            gm.NextDaySequence();
            gm.SetGameState(GameState.DayOver);
        }
        audioSource.Pause();
        currentCall = null;
        animationPlaying = false;
         
        AudioManager.instance.EndSequenceMix();
    }   
    private IEnumerator delayNewSlide(float time, Sprite slide)
    {
        yield return new WaitForSeconds(time);
        slideDisplayer.transform.GetChild(2).GetChild(0).GetComponent<Image>().sprite = slide;
        animationPlaying = false;

        
    }
    public void BlackScreen()
    {
        blackScreen.transform.parent.GetComponent<Animator>().SetBool("on", true);
    } 
    public void BlackScreenOff()
    {
        blackScreen.transform.parent.GetComponent<Animator>().SetBool("on", false);
    } 

    private IEnumerator pauseSequence(float time)
    {
        yield return new WaitForSeconds(time);
        
        slideDisplayer.transform.parent.Find("MousePrompt").gameObject.SetActive(true);
        audioSource.Stop(); 
    }

    public bool CancelSequence()
    {
        if (animationPlaying)
        {
            return false;
        }
        if (sequencePlaying)
        {
            StopAllCoroutines();
            audioSource.Stop();
            gm.SetGameState(GameState.Playing);
            subtitleText.text = "";
            if(blackScreen.transform.parent.GetComponent<Animator>().GetBool("onPhone")){
                blackScreen.transform.parent.GetComponent<Animator>().SetBool("onPhone", false);}
            else{
                blackScreen.transform.parent.GetComponent<Animator>().SetBool("on", false);
            }
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
            AudioManager.instance.EndSequenceMix();
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

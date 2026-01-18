using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phone : MonoBehaviour
{
    [SerializeField] private AudioClip phoneRing;
    [SerializeField] private AudioClip phonePickup;
    [SerializeField] private AudioClip phoneHangup;

    private Narration narration;

    [SerializeField] private PhoneHighlight phoneHighlight;
    [SerializeField] private string callName;

    [SerializeField] private bool isRinging = false;

    private GameManager gm;
    private AudioManager am;

    private bool wasOnPc = false;

    private Animator animator;
    private string lastCallName;

    // TODO: maybe create a scriptable object for the phone calls for each day

    public bool GetIsRinging() { return isRinging; }
    void Start()
    {
        phoneHighlight.gameObject.SetActive(false);
        narration = FindObjectOfType<Narration>();
        animator = GetComponent<Animator>();
        gm = GameManager.instance;
        am = AudioManager.instance;
        gm.getPinboard().PinAdded.AddListener(Ring);
        gm.InvestigationStateChanged.AddListener(ExitTutorial);
    }
    public void ExitTutorial()
    {
        if (gm.GetAnswerCommited() && gm.GetDay() == 1)
        {
            Ring("exit");
            gm.InvestigationStateChanged.RemoveListener(ExitTutorial);
        }
    }

    public void Ring()
    {
        if (gm.GetGameState() == GameState.OnPC && gm.GetDay() == 2)
        {
            wasOnPc = true;
            ResetPhone();
        }
        if (gm.GetDay() == 2 && !isRinging && !gm.GetAnswerCommited())
        {
            wasOnPc = false; 
            string callName = "phoneCallIntro";//FindObjectOfType<Pinboard>().tutorialElementOnBoard();
            Ring(callName);
            gm.getPinboard().PinAdded.RemoveListener(Ring);
            if (callName == "phoneCallIntro")   
            {
                gm.StateChanged.RemoveListener(Ring);
            }
        }
    }
    public void Ring(string callName)
    {
        lastCallName = callName;
        animator.Play("Ringing");
        isRinging = true; 
        phoneHighlight.gameObject.SetActive(true);
        phoneHighlight.StartCoroutine(phoneHighlight.PhoneRinging(transform));
        this.callName = callName;
        am.PlayAudioRepeating(phoneRing, volume: 0.5f, parent: gameObject);
    }

    public void StartCall()
    {
        if (!isRinging)
        {
            if (!narration.CancelSequence())
            {
               // gm.StateChanged.AddListener(Ring);
                if (lastCallName != null)
                {
                    narration.Say(lastCallName+"_replay");
                }
                else
                {
                    narration.Say("phoneNotWorking");
                }
            }
            return;
        }
        am.StopAudioRepeating(phoneRing);
        Quaternion[] rotations = new Quaternion[2];
        rotations[0] = Quaternion.identity;
        rotations[1] = "phoneCallIntro" == callName ? Quaternion.Euler(-20, -100, 0) : Quaternion.identity;
        // Find object with component pinboard 
        // FindObjectOfType<Pinboard>().AddTutorialRelevantObjects();

        //rotations[2] = Quaternion.Euler(-20, -70, 0);


        phoneHighlight.StopHighlight();
        narration.PlaySequence(callName, rotations);
        isRinging = false;
        animator.Play("Pickup");
        //  StartCoroutine(resetPhone());
    }

    public void ResetPhone()
    {
        isRinging = false;
        animator.Play("Phone");
        am.StopAudioRepeating(phoneRing);
    }


    // Update is called once per frame
    void Update()
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Phone : MonoBehaviour
{
    [SerializeField] private AudioClip phoneRing;
    [SerializeField] private AudioClip phonePickup;
    [SerializeField] private AudioClip phoneHangup;

    private AudioSource audioSource;
    private Narration narration;

    private string callName;

    [SerializeField] private bool isRinging = false;

    private GameManager gm;

    private bool wasOnPc = false;

    private Animator animator;

    // TODO: maybe create a scriptable object for the phone calls for each day

    public bool GetIsRinging() { return isRinging; }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        narration = FindObjectOfType<Narration>();
        animator = GetComponent<Animator>();
        gm = GameManager.instance;
        gm.StateChanged.AddListener(Ring);
    }

    public void Ring()
    {
        if (gm.GetGameState() == GameState.OnPC)
        {
            wasOnPc = true;
            ResetPhone();
        }
        if (gm.GetGameState() == GameState.Playing && wasOnPc && gm.GetDay() == 1 && !isRinging)
        {
            wasOnPc = false;
            string callName = FindObjectOfType<Pinboard>().tutorialElementOnBoard();
            Ring(callName);
            if (callName == "phoneCallIntro")
            {
                gm.StateChanged.RemoveListener(Ring);
            }
        }
    }
    public void Ring(string callName)
    {
        animator.Play("Ringing");
        isRinging = true;
        this.callName = callName;
        audioSource.clip = phoneRing;
        audioSource.Play();
    }

    public void StartCall()
    {
        if (!isRinging)
        {
            narration.Say("phoneNotWorking");
            return;
        }
        audioSource.Stop();
        Quaternion[] rotations = new Quaternion[2];
        rotations[0] = Quaternion.identity;
        rotations[1] = "phoneCallIntro" == callName ? Quaternion.Euler(-20, -100, 0) : Quaternion.identity;
        // Find object with component pinboard 
        // FindObjectOfType<Pinboard>().AddTutorialRelevantObjects();

        //rotations[2] = Quaternion.Euler(-20, -70, 0);

        narration.PlaySequence(callName, rotations);
        isRinging = false;
        animator.Play("Pickup");
        //  StartCoroutine(resetPhone());
    }

    public void ResetPhone()
    {
        isRinging = false;
        animator.Play("Phone");
        audioSource.Stop();
    }


    // Update is called once per frame
    void Update()
    {
    }
}

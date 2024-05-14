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
        }
        if (gm.GetGameState() == GameState.Playing && wasOnPc)
        {
            Ring("phoneCallIntro");
            gm.StateChanged.RemoveListener(Ring);
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
            return;

        audioSource.Stop();
        /* audioSource.clip = phonePickup;
         audioSource.Play();*/
        Quaternion[] rotations = new Quaternion[3];
        rotations[0] = Quaternion.identity;
        rotations[1] = Quaternion.Euler(-20, -100, 0);
        rotations[2] = Quaternion.Euler(-20, -70, 0);

        narration.PlaySequence(callName, rotations);
        isRinging = false;
        animator.Play("Pickup");
        StartCoroutine(resetPhone());
    }

    private IEnumerator resetPhone()
    {
        yield return new WaitForSeconds(10);
        animator.Play("Phone");
    }


    // Update is called once per frame
    void Update()
    {

    }
}

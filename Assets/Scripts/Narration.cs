using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narration : MonoBehaviour
{

    [SerializeField] private AudioClip notLeavingVoice;
    [SerializeField] private AudioClip positiveFeedbackVoice;
    [SerializeField] private AudioClip negativeFeedbackVoice;
    [SerializeField] private AudioClip deletePostVoice;
    [SerializeField] private AudioClip suspectFoundVoice;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Say(string text)
    {
        switch (text)
        {
            case "notLeaving":
                audioSource.clip = notLeavingVoice;
                break;
            case "positiveFeedback":
                audioSource.clip = positiveFeedbackVoice;
                break;
            case "negativeFeedback":
                audioSource.clip = negativeFeedbackVoice;
                break;
            case "deletePost":
                audioSource.clip = deletePostVoice;
                break;
            case "suspectFound":
                audioSource.clip = suspectFoundVoice;
                break;
        }
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
}

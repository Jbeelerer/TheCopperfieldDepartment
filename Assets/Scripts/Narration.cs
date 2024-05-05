using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    private TextMeshProUGUI subtitleText;

    private AudioSource audioSource;

    private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        gm.SetNarration(this);
        audioSource = GetComponent<AudioSource>();
        subtitleText = GameObject.Find("Subtitle").GetComponent<TextMeshProUGUI>();
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
}

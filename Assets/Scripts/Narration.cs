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
    [SerializeField] private AudioClip introClip;
    private Dictionary<string, float> intro = new Dictionary<string, float>() { { "Good Morning!", 1f }, { "Happy to have you as part of our team!!", 2.1f }, { "I would like to remind you of the secrecy clause in your contract!", 4.2f }, { "The existence of The Copperfield Department is highly confidential.", 4.2f }, { "Any breach of contract will waive your claim to physical integrity.", 5f }, { "Mister Tery Waldberg will be in touch with you regarding your first assignment.", 4.6f }, { "Have a productive day!", 1.5f } };

    private TextMeshProUGUI subtitleText;

    private AudioSource audioSource;

    private GameManager gm;

    private GameObject blackScreen;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        gm.SetNarration(this);
        audioSource = GetComponent<AudioSource>();
        subtitleText = GameObject.Find("Subtitle").GetComponent<TextMeshProUGUI>();

        blackScreen = GameObject.Find("BlackScreen");
        blackScreen.SetActive(false);

        if (gm.GetDay() == 1)
        {
            StartIntro();
        }


    }
    private void StartIntro()
    {
        gm.SetGameState(GameState.Frozen);
        audioSource.clip = introClip;
        audioSource.Play();
        StartCoroutine(PlaySequence(intro));
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

    private IEnumerator PlaySequence(Dictionary<string, float> content)
    {
        print("Playing sequence");
        blackScreen.SetActive(true);
        subtitleText.fontSize += 10;
        foreach (KeyValuePair<string, float> entry in content)
        {
            subtitleText.text = entry.Key;
            yield return new WaitForSeconds(entry.Value);

        }

        gm.SetGameState(GameState.Playing);
        subtitleText.text = "";
        subtitleText.fontSize -= 10;
        blackScreen.SetActive(false);
    }
}

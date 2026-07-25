using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class FeedbackForm : MonoBehaviour
{
    [SerializeField] GameObject message;
    [SerializeField] GameObject form;
    [SerializeField] GameObject success;
    private int rating = 0;
    [SerializeField] TMP_InputField liked ;
    [SerializeField] TMP_InputField disliked;

    [SerializeField] private AudioClip DramaticHit1;
    [SerializeField] private AudioClip DramaticHit2;
    [SerializeField] private AudioClip PaperSound;
    static string url = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSfCG5pp32DBLsfTYhQ2MIVmU1Scle8FtYXlEd_9nbGHAk5Z8A/formResponse";

    private AudioManager am;

    // Start is called before the first frame update
    void Start()
    {
         Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        am = FindFirstObjectByType<AudioManager>();
    }

    // Update is called once per frame
    public void SetRating(int r)
    {
     rating = int.Parse(r.ToString());   
    }
    public void ProceedToForm()
    {
        //message.SetActive(false);
        form.SetActive(true);
        am.PlayAudio(PaperSound, 0.7f);
    }
    public void Send()
    { 
        StartCoroutine(SendFeedback(rating, disliked.text, liked.text));
        //form.SetActive(false);
        success.SetActive(true);
        am.PlayAudio(PaperSound, 0.7f);
    }
    IEnumerator SendFeedback(int rating, string disliked, string liked)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.1328074360", rating);
        form.AddField("entry.107984603", liked);
        form.AddField("entry.1174536770", disliked);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void PlaySoundDuringAnimation(AudioClip clip)
    {
        if (am == null)
        {
            am = FindFirstObjectByType<AudioManager>();
        }
        am.PlayAudio(clip);
    }
}

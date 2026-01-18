using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FeedbackForm : MonoBehaviour
{
    [SerializeField] GameObject form;
    [SerializeField] GameObject success;
    private int rating = 0;
    [SerializeField] TMP_InputField liked ;
    [SerializeField] TMP_InputField disliked;  
    static string url = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSfCG5pp32DBLsfTYhQ2MIVmU1Scle8FtYXlEd_9nbGHAk5Z8A/formResponse";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetRating(int r)
    {
     rating = int.Parse(r.ToString());   
    }
    public void Send()
    { 
        StartCoroutine(SendFeedback(rating, disliked.text, liked.text));
        form.SetActive(false);
        success.SetActive(true);
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
}

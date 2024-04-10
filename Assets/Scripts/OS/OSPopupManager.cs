using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSPopupManager : MonoBehaviour
{
    private Animator anim;
    private TextMeshProUGUI messageText;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        messageText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    public void DisplayPostPinMessage()
    {
        messageText.text = "Post added to pinboard!";
        anim.Play("popupMessageSpawn", -1, 0);
    }

    public void DisplayUserPinMessage()
    {
        messageText.text = "User added to pinboard!";
        anim.Play("popupMessageSpawn", -1, 0);
    }

    public void DisplayPersonPinMessage()
    {
        messageText.text = "Person added to pinboard!";
        anim.Play("popupMessageSpawn", -1, 0);
    }

    public void DisplayPersonAccusedMessage()
    {
        messageText.text = "Marked person as prime suspect!";
        anim.Play("popupMessageSpawn", -1, 0);
    }

    public void DisplayPostDeleteMessage()
    {
        messageText.text = "Post flagged for deletion!";
        anim.Play("popupMessageSpawn", -1, 0);
    }
}

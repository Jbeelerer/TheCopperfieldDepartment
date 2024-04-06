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

    public void DisplayPersonDetainedMessage()
    {
        messageText.text = "Made person disappear (magically)!";
        anim.Play("popupMessageSpawn", -1, 0);
    }

    public void DisplayPostDeleteMessage()
    {
        messageText.text = "Cannot delete posts right now";
        anim.Play("popupMessageSpawn", -1, 0);
    }
}

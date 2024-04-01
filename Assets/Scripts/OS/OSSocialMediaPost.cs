using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSSocialMediaPost : MonoBehaviour
{
    public SocialMediaPost post;

    private Pinboard pinboard;
    private OSPopupManager popupManager;

    // Start is called before the first frame update
    void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();
    }

    public void instanctiatePost(SocialMediaPost post)
    {
        // Instantiate the post
        this.post = post;
    }
    public void AddPostToPinboard(string type)
    {
        switch (type)
        {
            case "name":
                pinboard.AddPin(post.author);
                popupManager.DisplayUserPinMessage();
                break;
            case "content":
                popupManager.DisplayPostPinMessage();
                pinboard.AddPin(post.author);
                pinboard.AddPin(post);
                break;
            case "person":
                pinboard.AddPin("Person");
                break;
        }
    }
}

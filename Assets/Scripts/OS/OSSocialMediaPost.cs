using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OSSocialMediaPost : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SocialMediaPost post;

    private Pinboard pinboard;
    private OSPopupManager popupManager;
    private GameObject postOptions;

    // Start is called before the first frame update
    void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();
        postOptions = transform.Find("PostOptions").gameObject;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        postOptions.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        postOptions.SetActive(false);
    }
}

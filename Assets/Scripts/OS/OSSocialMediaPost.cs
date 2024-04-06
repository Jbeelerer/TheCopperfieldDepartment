using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSSocialMediaPost : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SocialMediaPost post;

    private Pinboard pinboard;
    private OSPopupManager popupManager;
    private GameObject postOptions;

    private bool postPinned = false;
    private bool userPinned = false;

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
                userPinned = true;
                postOptions.transform.Find("PinUser").GetComponent<Image>().color = Color.red;
                break;
            case "content":
                popupManager.DisplayPostPinMessage();
                pinboard.AddPin(post.author);
                pinboard.AddPin(post);
                postPinned = true;
                postOptions.transform.Find("PinPost").GetComponent<Image>().color = Color.red;
                break;
            case "person":
                pinboard.AddPin("Person");
                break;
        }
    }

    public void DeletePost()
    {
        popupManager.DisplayPostDeleteMessage();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Transform option in postOptions.transform)
        {
            option.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (Transform option in postOptions.transform)
        {
            option.gameObject.SetActive(false);

            if ((option.name == "PinPost" && postPinned)
                || (option.name == "PinUser" && userPinned))
            {
                option.gameObject.SetActive(true);
            }
        }
    }
}

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
    private FPSController fpsController;
    private GameManager gm;
    private OSSocialMediaContent socialMediaContent;
    private ComputerControls computerControls;

    private bool postPinned = false;
    private bool userPinned = false;

    // Start is called before the first frame update
    void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();
        gm = GameManager.instance;
        postOptions = transform.Find("PostOptions").gameObject;
        fpsController = GameObject.Find("Player").GetComponent<FPSController>();
        socialMediaContent = transform.GetComponentInParent<OSSocialMediaContent>();
        computerControls = transform.GetComponentInParent<ComputerControls>();

        fpsController.OnPinDeletion.AddListener(RemovePinned);
    }

    private void RemovePinned(ScriptableObject so)
    {
        switch (so)
        {
            case SocialMediaPost:
                if (so == post)
                {
                    postOptions.transform.Find("PinPost").GetComponent<Image>().color = Color.black;
                    postPinned = false;
                }
                break;
            case SocialMediaUser:
                if (so == post.author)
                {
                    postOptions.transform.Find("PinUser").GetComponent<Image>().color = Color.black;
                    userPinned = false;
                }
                break;
        }
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
                popupManager.DisplayUserPinMessage();
                pinboard.AddPin(post.author);
                socialMediaContent.AddToPinnedUserList(post.author);
                userPinned = true;
                postOptions.transform.Find("PinUser").GetComponent<Image>().color = Color.red;
                break;
            case "content":
                popupManager.DisplayPostPinMessage();
                pinboard.AddPin(post.author);
                socialMediaContent.AddToPinnedUserList(post.author);
                userPinned = true;
                postOptions.transform.Find("PinUser").GetComponent<Image>().color = Color.red;
                pinboard.AddPin(post);
                postPinned = true;
                postOptions.transform.Find("PinPost").GetComponent<Image>().color = Color.red;
                break;
        }
    }

    public void DeletePost()
    {
        computerControls.OpenWindow(OSAppType.WARNING, "You are about to flag this post for deletion. Any currently accused person will be undone.", DeletePostSuccess);
    }

    private void DeletePostSuccess()
    {
        socialMediaContent.ClearDeletedPost();
        computerControls.investigationState = OSInvestigationState.POST_DELETED;
        if (computerControls.GetComponentInChildren<OSPeopleListContent>())
        {
            computerControls.GetComponentInChildren<OSPeopleListContent>().ClearAccusedPeople();
        }
        popupManager.DisplayPostDeleteMessage();
        postOptions.transform.Find("DeletePost").GetComponent<Image>().color = Color.red;
        gm.checkDeletedPost(post);
    }

    public void OpenProfile()
    {
        socialMediaContent.ShowUserProfile(post.author);
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

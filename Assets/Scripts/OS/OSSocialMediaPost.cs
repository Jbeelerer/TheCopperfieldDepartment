using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSSocialMediaPost : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerMoveHandler
{
    public SocialMediaPost post;

    private Pinboard pinboard;
    private OSPopupManager popupManager;
    private GameObject postOptions;
    private FPSController fpsController;
    private GameManager gm;
    private OSSocialMediaContent socialMediaContent;
    private ComputerControls computerControls;
    private Camera canvasCam;

    private bool postPinned = false;
    private bool userPinned = false;
    private bool postDeleted = false;

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
        canvasCam = GameObject.Find("computerTextureCam").GetComponent<Camera>();

        fpsController.OnPinDeletion.AddListener(RemovePinned);
        socialMediaContent.OnPinned.AddListener(MarkPinned);
        computerControls.OnUnpinned.AddListener(RemovePinned);
        socialMediaContent.OnDeletedPostClear.AddListener(ClearDeleted);
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

                    popupManager.DisplayPostUnpinMessage();
                }
                break;
            case SocialMediaUser:
                if (so == post.author)
                {
                    postOptions.transform.Find("PinUser").GetComponent<Image>().color = Color.black;
                    userPinned = false;

                    popupManager.DisplayUserUnpinMessage();
                }
                break;
        }
    }

    private void MarkPinned(ScriptableObject so)
    {
        switch (so)
        {
            case SocialMediaPost:
                // Pin specific post and its user
                if (((SocialMediaPost)so).author == post.author)
                {
                    postOptions.transform.Find("PinUser").GetComponent<Image>().color = Color.red;
                    userPinned = true;
                    pinboard.AddPin(post.author);
                    socialMediaContent.AddToPinnedUserList(post.author);
                }
                if (so == post)
                {
                    postOptions.transform.Find("PinPost").GetComponent<Image>().color = Color.red;
                    postPinned = true;
                    pinboard.AddPin(post);

                    popupManager.DisplayPostPinMessage();
                }
                break;
            case SocialMediaUser:
                // Pin only user
                if (so == post.author)
                {
                    postOptions.transform.Find("PinUser").GetComponent<Image>().color = Color.red;
                    userPinned = true;
                    pinboard.AddPin(post.author);
                    socialMediaContent.AddToPinnedUserList(post.author);

                    popupManager.DisplayUserPinMessage();
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
                if (!userPinned)
                {
                    socialMediaContent.PinPost(type, post);
                }
                else
                {
                    socialMediaContent.UnpinPost(type, post);
                }
                break;
            case "content":
                if (!postPinned)
                {
                    socialMediaContent.PinPost(type, post);
                }
                else
                {
                    socialMediaContent.UnpinPost(type, post);
                }
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
        postDeleted = true;
        gm.checkDeletedPost(post);
    }

    private void ClearDeleted()
    {
        postOptions.transform.Find("DeletePost").GetComponent<Image>().color = Color.black;
        postDeleted = false;
    }

    public void OpenProfile()
    {
        socialMediaContent.ShowUserProfile(post.author);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TextMeshProUGUI textMesh = transform.Find("content").GetComponent<TextMeshProUGUI>();
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMesh, eventData.position, canvasCam);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textMesh.textInfo.linkInfo[linkIndex];
            socialMediaContent.FilterHomefeed(linkInfo.GetLinkText());
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Transform option in postOptions.transform)
        {
            option.gameObject.SetActive(true);
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(transform.Find("content").GetComponent<TMP_Text>(), eventData.position, canvasCam);
        if (computerControls.GetFirstHitObject() && !computerControls.GetFirstHitObject().GetComponent<Button>())
        {
            computerControls.cursor.GetComponent<Image>().sprite = linkIndex != -1 ? computerControls.cursorClickable : computerControls.cursorNormal;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (Transform option in postOptions.transform)
        {
            option.gameObject.SetActive(false);

            if ((option.name == "PinPost" && postPinned)
                || (option.name == "PinUser" && userPinned)
                || option.name == "DeletePost" && postDeleted)
            {
                option.gameObject.SetActive(true);
            }
        }
    }
}

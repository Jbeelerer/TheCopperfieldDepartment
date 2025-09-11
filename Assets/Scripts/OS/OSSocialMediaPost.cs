using TMPro;
using UnityEngine;
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

    private Sprite pinUserDefaultSprite;
    private Sprite pinPostDefaultSprite;
    private Sprite DeletePostDefaultSprite;

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
        socialMediaContent.OnDeletedRefresh.AddListener(MarkDeleted);
        computerControls.OnUnpinned.AddListener(RemovePinned);
        socialMediaContent.OnDeletedPostClear.AddListener(ClearDeleted);

        pinUserDefaultSprite = postOptions.transform.Find("PinUser").GetComponent<Image>().sprite;
        pinPostDefaultSprite = postOptions.transform.Find("PinPost").GetComponent<Image>().sprite;
        DeletePostDefaultSprite = postOptions.transform.Find("DeletePost").GetComponent<Image>().sprite;
    }

    private void RemovePinned(ScriptableObject so)
    {
        switch (so)
        {
            case SocialMediaPost:
                if (so == post)
                {
                    postOptions.transform.Find("PinPost").GetComponent<Image>().sprite = pinPostDefaultSprite;
                    //postOptions.transform.Find("PinPost").gameObject.SetActive(false);
                    postPinned = false;

                    popupManager.DisplayPostUnpinMessage();
                }
                break;
            case SocialMediaUser:
                if (so == post.author)
                {
                    postOptions.transform.Find("PinUser").GetComponent<Image>().sprite = pinUserDefaultSprite;
                    if (gameObject != socialMediaContent.currentFocusedPost)
                    {
                        postOptions.transform.Find("PinUser").gameObject.SetActive(false);
                    }
                    userPinned = false;
                    socialMediaContent.RemovePinnedUser(post.author);

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
                if (((SocialMediaPost)so).author == post.author && post.author != null)
                {
                    postOptions.transform.Find("PinUser").GetComponent<Image>().sprite = postOptions.transform.Find("PinUser").GetComponent<Button>().spriteState.pressedSprite;
                    postOptions.transform.Find("PinUser").gameObject.SetActive(true);
                    userPinned = true;
                    pinboard.AddPin(post.author);
                    socialMediaContent.AddToPinnedUserList(post.author);
                }
                if (so == post)
                {
                    postOptions.transform.Find("PinPost").GetComponent<Image>().sprite = postOptions.transform.Find("PinPost").GetComponent<Button>().spriteState.pressedSprite;
                    postOptions.transform.Find("PinPost").gameObject.SetActive(true);
                    postPinned = true;
                    pinboard.AddPin(post);

                    popupManager.DisplayPostPinMessage();
                }
                break;
            case SocialMediaUser:
                // Pin only user
                if (so == post.author)
                {
                    postOptions.transform.Find("PinUser").GetComponent<Image>().sprite = postOptions.transform.Find("PinUser").GetComponent<Button>().spriteState.pressedSprite;
                    postOptions.transform.Find("PinUser").gameObject.SetActive(true);
                    userPinned = true;
                    pinboard.AddPin(post.author);
                    socialMediaContent.AddToPinnedUserList(post.author);

                    popupManager.DisplayUserPinMessage();
                }
                break;
        }
    }

    private void MarkDeleted(ScriptableObject so)
    {
        if ((SocialMediaPost)so == post)
        {
            postOptions.transform.Find("DeletePost").GetComponent<Image>().sprite = postOptions.transform.Find("DeletePost").GetComponent<Button>().spriteState.pressedSprite;
            postOptions.transform.Find("DeletePost").gameObject.SetActive(true);
            postDeleted = true;
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
        if (!postDeleted)
        {
            computerControls.OpenWindow(OSAppType.WARNING, "You are about to flag this post for deletion.<br><br><b>This can still be changed later.</b>", DeletePostSuccess);
        }
        else
        {
            socialMediaContent.ClearDeletedPost();
            popupManager.DisplayPostUndeleteMessage();
        }
    }

    private void DeletePostSuccess()
    {
        socialMediaContent.ClearDeletedPost();
        computerControls.investigationState = OSInvestigationState.POST_DELETED;
        popupManager.DisplayPostDeleteMessage();
        socialMediaContent.RefreshDeletedPost(post);
        gm.checkDeletedPost(post);
    }

    private void ClearDeleted()
    {
        postOptions.transform.Find("DeletePost").GetComponent<Image>().sprite = DeletePostDefaultSprite;
        if (gameObject != socialMediaContent.currentFocusedPost)
        {
            postOptions.transform.Find("DeletePost").gameObject.SetActive(false);
        }
        postDeleted = false;
    }

    public void OpenProfile()
    {
        socialMediaContent.ShowUserProfile(post.author);
    }

    public void OpenImageInWindow()
    {
        computerControls.OpenWindow(OSAppType.IMAGE, imagePost: post);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TextMeshProUGUI textMesh = transform.Find("content").GetComponent<TextMeshProUGUI>();
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMesh, eventData.position, canvasCam);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textMesh.textInfo.linkInfo[linkIndex];
            if (linkInfo.GetLinkText().StartsWith("@"))
            {
                foreach (SocialMediaUser user in gm.GetUsers())
                {
                    if (user.username == linkInfo.GetLinkText().Substring(1))
                    {
                        socialMediaContent.ShowUserProfile(user);
                        return;
                    }
                }
            }
            else
            {
                socialMediaContent.FilterHomefeed(linkInfo.GetLinkText());
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Transform option in postOptions.transform)
        {
            option.gameObject.SetActive(true);
            // Disable post deletion option ALWAYS, because the feature is currently not used
            if (/*gm.GetDay() < 4 && */option.gameObject.name == "DeletePost")
            {
                option.gameObject.SetActive(false);
            }
        }
        socialMediaContent.currentFocusedPost = gameObject;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(transform.Find("content").GetComponent<TMP_Text>(), eventData.position, canvasCam);
        if (computerControls.GetFirstHitObject() && !computerControls.GetFirstHitObject().GetComponent<Button>())
        {
            computerControls.cursor.GetComponent<Image>().sprite = linkIndex != -1 ? computerControls.cursorClickable : computerControls.cursorNormal;
            computerControls.isHoveringOverLink = linkIndex != -1;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Keep post options open if pointy tutorial is active
        if (computerControls.pointySystem.GetIsPointyActive())
            return;

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
        socialMediaContent.currentFocusedPost = null;
    }
}

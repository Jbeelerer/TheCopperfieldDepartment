using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class PinEvent : UnityEvent<ScriptableObject> { }

public class OSSocialMediaContent : MonoBehaviour
{
    [SerializeField] private GameObject socialMediaPostContainer;
    [SerializeField] private GameObject postPrefab;
    [SerializeField] private GameObject conversationPrefab;
    [SerializeField] private GameObject dmPrefabLeft;
    [SerializeField] private GameObject dmPrefabRight;
    [SerializeField] private GameObject homePage;
    [SerializeField] private GameObject homePageContent;
    [SerializeField] private GameObject profilePage;
    [SerializeField] private GameObject profilePageContent;
    [SerializeField] private Transform profilePageheader;
    [SerializeField] private GameObject conversationsPage;
    [SerializeField] private GameObject conversationsPageContent;
    [SerializeField] private GameObject dmPage;
    [SerializeField] private GameObject dmPageContent;
    [SerializeField] private Transform searchBar;
    [SerializeField] private GameObject searchHistoryContent;
    [SerializeField] private GameObject searchTermPrefab;
    [SerializeField] private Sprite searchTermIconHashtag;
    [SerializeField] private Sprite searchTermIconUser;

    private int postNumber = 1;
    private List<OSSocialMediaPost> postList = new List<OSSocialMediaPost>();
    private ComputerControls computerControls;
    private List<SocialMediaUser> pinnedUsers = new List<SocialMediaUser>();
    private List<SocialMediaUser> usersWithFoundPassword = new List<SocialMediaUser>();
    private FPSController fpsController;
    private OSPopupManager popupManager;
    public SocialMediaUser currentUser;

    public GameObject currentFocusedPost = null;

    public PinEvent OnPinned;
    public PinEvent OnDeletedRefresh;
    public UnityEvent OnDeletedPostClear;

    // Start is called before the first frame update
    void Awake()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
    }

    private void Start()
    {
        fpsController = GameObject.Find("Player").GetComponent<FPSController>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();
        fpsController.OnPinDeletion.AddListener(RemovePinnedUser);
        StartCoroutine(InstanciateContent());
    }
    private IEnumerator InstanciateContent()
    {
        yield return new WaitForSeconds(1);
        foreach (SocialMediaPost s in computerControls.GetPosts())
        {
            InstanciatePost(s);
        }

        foreach (DMConversation convo in computerControls.GetConversations())
        {
            InstantiateConversation(convo);
        }

        ResetHomeFeed();
    }

    private void OnEnable()
    {
        if (computerControls.investigationState == OSInvestigationState.PERSON_ACCUSED)
        {
            ClearDeletedPost();
        }
    }

    public void InstanciatePost(SocialMediaPost post)
    {
        GameObject newPost = Instantiate(postPrefab, socialMediaPostContainer.transform);
        newPost.GetComponent<OSSocialMediaPost>().instanctiatePost(post);
        newPost.name = "Post" + postNumber;
        postNumber++;
        newPost.transform.Find("TopRow").Find("ProfilePic").Find("imageMask").GetChild(0).GetComponent<Image>().sprite = post.author.image;
        newPost.transform.Find("TopRow").Find("name").GetComponent<TextMeshProUGUI>().text = post.author.username;
        newPost.transform.Find("TopRow").Find("TimeStamp").GetComponent<TextMeshProUGUI>().text = post.time;
        newPost.transform.Find("content").GetComponent<TextMeshProUGUI>().text = post.content;
        if (post.image)
        {
            newPost.transform.Find("ImageContainer").Find("AttachedImage").GetComponent<Image>().preserveAspect = true;
            newPost.transform.Find("ImageContainer").Find("AttachedImage").GetComponent<Image>().sprite = post.image;
        }
        else
        {
            newPost.transform.Find("ImageContainer").gameObject.SetActive(false);
        }
        newPost.transform.Find("ForbiddenOptions").Find("Comments").GetComponentInChildren<TextMeshProUGUI>().text = GetRandomEngagementNumber(post.author.popularityLevel);
        newPost.transform.Find("ForbiddenOptions").Find("Shares").GetComponentInChildren<TextMeshProUGUI>().text = GetRandomEngagementNumber(post.author.popularityLevel);
        newPost.transform.Find("ForbiddenOptions").Find("Likes").GetComponentInChildren<TextMeshProUGUI>().text = GetRandomEngagementNumber(post.author.popularityLevel);
        Instantiate(newPost, profilePageContent.transform);
        postList.Add(newPost.GetComponent<OSSocialMediaPost>());
    }

    private void InstantiateConversation(DMConversation convo)
    {
        GameObject newConvo = Instantiate(conversationPrefab, conversationsPageContent.transform);
        newConvo.GetComponent<OSConversation>().InstantiateConversation(convo);
        newConvo.name = "Convo" + convo.id;
    }

    private string GetRandomEngagementNumber(PopularityLevel popularityLevel)
    {
        switch (popularityLevel)
        {
            case PopularityLevel.LOW:
                return Random.Range(1, 25).ToString();
            case PopularityLevel.MEDIUM:
                return Random.Range(50, 200).ToString();
            case PopularityLevel.HIGH:
                return Random.Range(1, 25).ToString() + "K";
            case PopularityLevel.VERY_HIGH:
                return Random.Range(25, 50).ToString() + "K";
            default:
                return "0";
        }
    }

    public void EnableFirstPostOptions()
    {
        postList[0].OnPointerEnter(null);
    }

    public void PinPost(string type, SocialMediaPost post)
    {
        switch (type)
        {
            case "name":
                OnPinned?.Invoke(post.author);
                break;
            case "content":
                OnPinned?.Invoke(post);
                break;
        }
    }

    public void UnpinPost(string type, SocialMediaPost post)
    {
        switch (type)
        {
            case "name":
                computerControls.OnUnpinned?.Invoke(post.author);
                break;
            case "content":
                computerControls.OnUnpinned?.Invoke(post);
                break;
        }
    }

    public void ClearDeletedPost()
    {
        OnDeletedPostClear?.Invoke();
    }

    public void RefreshDeletedPost(SocialMediaPost post)
    {
        OnDeletedRefresh?.Invoke(post);
    }

    public void FilterHomefeed(string filterTerm)
    {
        ShowHomeFeed();

        ChangeSearchBar("Searching: <b>" + filterTerm + "</b>", true, ResetHomeFeed);

        foreach (Transform post in socialMediaPostContainer.transform)
        {
            if (post.GetComponent<OSSocialMediaPost>().post.content.Contains(filterTerm))
            {
                post.gameObject.SetActive(true);
            }
            else
            {
                post.gameObject.SetActive(false);
            }
        }

        // Add filter term to search history
        string termWithoutHash = filterTerm.Substring(1);
        foreach (Transform term in searchHistoryContent.transform)
        {
            if (term.Find("Name").GetComponent<TextMeshProUGUI>().text == termWithoutHash)
            {
                term.SetAsLastSibling();
                return;
            }
        }
        GameObject newSearchTerm = Instantiate(searchTermPrefab, searchHistoryContent.transform);
        newSearchTerm.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = termWithoutHash;
        newSearchTerm.transform.Find("Icon").GetComponent<Image>().sprite = searchTermIconHashtag;
    }

    public void ResetHomeFeed()
    {
        ChangeSearchBar("Home", false);

        // Display all posts that aren't flagged to be hidden in the home feed
        foreach (Transform post in socialMediaPostContainer.transform)
        {
            if (!post.GetComponent<OSSocialMediaPost>().post.hiddenInHomeFeed)
            {
                post.gameObject.SetActive(true);
            }
            else
            {
                post.gameObject.SetActive(false);
            }
        }

        socialMediaPostContainer.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 1;
    }

    public void ShowHomeFeed()
    {
        ChangeSearchBar("Home", false);

        currentUser = null;
        homePage.transform.SetAsLastSibling();
    }

    public void ShowUserProfile(SocialMediaUser user)
    {
        ChangeSearchBar("Profile View", false, ShowHomeFeed);

        currentUser = user;
        //profilePage.SetActive(true);
        profilePage.transform.SetAsLastSibling();

        profilePage.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;

        profilePageheader.Find("Banner").Find("BannerMask").Find("Image").GetComponent<Image>().sprite = user.profileBanner;
        profilePageheader.Find("ProfilePic").Find("ImageMask").Find("Image").GetComponent<Image>().sprite = user.image;
        profilePageheader.Find("Name").GetComponent<TextMeshProUGUI>().text = user.username;
        profilePageheader.Find("Description").GetComponent<TextMeshProUGUI>().text = user.bioText;
        if (pinnedUsers.Contains(user))
        {
            profilePageheader.Find("PinUser").GetComponent<Image>().color = Color.red;
        }
        else
        {
            profilePageheader.Find("PinUser").GetComponent<Image>().color = Color.white;
        }

        // Show only posts from current user profile
        foreach (Transform post in profilePageContent.transform)
        {
            if (post.GetComponent<OSSocialMediaPost>())
            {
                if (post.GetComponent<OSSocialMediaPost>().post.author == user)
                {
                    post.gameObject.SetActive(true);
                }
                else
                {
                    post.gameObject.SetActive(false);
                }
            }
        }

        // Add user to search history
        foreach (Transform term in searchHistoryContent.transform)
        {
            if (term.Find("Name").GetComponent<TextMeshProUGUI>().text == user.username)
            {
                term.SetAsLastSibling();
                return;
            }
        }
        GameObject newSearchTerm = Instantiate(searchTermPrefab, searchHistoryContent.transform);
        newSearchTerm.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = user.username;
        newSearchTerm.transform.Find("Icon").GetComponent<Image>().sprite = searchTermIconUser;
        newSearchTerm.GetComponent<OSSearchTerm>().user = user;
    }

    public void PinUser()
    {
        if (!pinnedUsers.Contains(currentUser))
        {
            OnPinned?.Invoke(currentUser);
        }
        else
        {
            computerControls.OnUnpinned?.Invoke(currentUser);
        }
    }

    public void RemovePinnedUser(ScriptableObject so)
    {
        if (so is not SocialMediaUser)
            return;

        if (pinnedUsers.Contains((SocialMediaUser)so))
        {
            pinnedUsers.Remove((SocialMediaUser)so);
        }
        if (so == currentUser)
        {
            profilePageheader.Find("PinUser").GetComponent<Image>().color = Color.white;
        }
    }

    public void AddToPinnedUserList(SocialMediaUser user)
    {
        if (!pinnedUsers.Contains(user))
        {
            pinnedUsers.Add(user);
        }
        if (user == currentUser)
        {
            profilePageheader.Find("PinUser").GetComponent<Image>().color = Color.red;
        }
    }

    public void ShowUserConversations()
    {
        if (!usersWithFoundPassword.Contains(currentUser))
        {
            computerControls.OpenWindow(OSAppType.WARNING, "You do not have the required credentials to log into this account!", hasCancelBtn: false);
            return;
        }

        ChangeSearchBar(currentUser.username + "'s DMs", false, CloseUserConversations);

        conversationsPage.transform.SetAsLastSibling();
        conversationsPage.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;

        // Show only conversations of current user
        foreach (Transform convo in conversationsPageContent.transform)
        {
            if (convo.GetComponent<OSConversation>())
            {
                if (convo.GetComponent<OSConversation>().conversation.conversationMember1 == currentUser || convo.GetComponent<OSConversation>().conversation.conversationMember2 == currentUser)
                {
                    convo.gameObject.SetActive(true);
                    convo.GetComponent<OSConversation>().MatchConvoToSenderView(currentUser);
                }
                else
                {
                    convo.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowLoginPopup()
    {
        if (usersWithFoundPassword.Contains(currentUser))
        {
            popupManager.DisplayAccountLoginMessage();
        }
    }

    public void CloseUserConversations()
    {
        ShowUserProfile(currentUser);
    }

    public void ShowUserDM(DMConversation convo)
    {
        ChangeSearchBar(currentUser.username + "'s DMs", false, ShowUserConversations);

        foreach (Transform dm in dmPageContent.transform)
        {
            Destroy(dm.gameObject);
        }
        convo.messages.ForEach(m =>
        {
            GameObject newDM = m.sender == currentUser ? Instantiate(dmPrefabRight, dmPageContent.transform) : Instantiate(dmPrefabLeft, dmPageContent.transform);
            newDM.GetComponent<OSDirectMessage>().InstantiateDM(m.message, m.timeStamp, m.image);
            newDM.name = "DM";
        });

        dmPage.transform.SetAsLastSibling();
    }

    private void ChangeSearchBar(string text, bool showSearchIcon, UnityAction backButtonFunc = null)
    {
        searchBar.Find("SearchText").GetComponent<TextMeshProUGUI>().text = text;
        searchBar.Find("BackButton").gameObject.SetActive(backButtonFunc != null);
        if (backButtonFunc != null)
        {
            searchBar.Find("BackButton").GetComponent<Button>().onClick.RemoveAllListeners();
            //UnityEvent backButtonEvent = new UnityEvent();
            //backButtonEvent.AddListener(backButtonFunc);
            searchBar.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => backButtonFunc());
        }
        searchBar.Find("Image").gameObject.SetActive(showSearchIcon);
    }

    public void AddUserWithFoundPassword(SocialMediaUser user)
    {
        usersWithFoundPassword.Add(user);
    }
}

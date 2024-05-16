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
    [SerializeField] private GameObject profilePage;
    [SerializeField] private GameObject profilePageContent;
    [SerializeField] private Transform profilePageheader;
    [SerializeField] private Transform searchBar;
    [SerializeField] private GameObject searchHistoryContent;
    [SerializeField] private GameObject searchTermPrefab;
    [SerializeField] private Sprite searchTermIconHashtag;
    [SerializeField] private Sprite searchTermIconUser;

    private int postNumber = 1;
    private List<OSSocialMediaPost> postList = new List<OSSocialMediaPost>();
    private ComputerControls computerControls;
    private SocialMediaUser currentUser;
    private List<SocialMediaUser> pinnedUsers = new List<SocialMediaUser>();
    private FPSController fpsController;

    public PinEvent OnPinned;
    public UnityEvent OnDeletedPostClear;

    // Start is called before the first frame update
    void Awake()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
    }

    private void Start()
    {
        fpsController = GameObject.Find("Player").GetComponent<FPSController>();
        fpsController.OnPinDeletion.AddListener(RemovePinnedUser);

        foreach (SocialMediaPost s in computerControls.GetPosts())
        {
            InstanciatePost(s);
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
        newPost.transform.Find("TopRow").Find("imageMask").GetChild(0).GetComponent<Image>().sprite = post.author.image;
        newPost.transform.Find("TopRow").Find("name").GetComponent<TextMeshProUGUI>().text = post.author.username;
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
        Instantiate(newPost, profilePageContent.transform);
        postList.Add(newPost.GetComponent<OSSocialMediaPost>());
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

    public void FilterHomefeed(string filterTerm)
    {
        CloseUserProfile();

        searchBar.Find("SearchText").GetComponent<TextMeshProUGUI>().text = "Searching for: <b>" + filterTerm + "</b>";
        searchBar.gameObject.SetActive(true);

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
        searchBar.gameObject.SetActive(false);

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
    }

    public void ShowUserProfile(SocialMediaUser user)
    {
        currentUser = user;
        profilePage.SetActive(true);

        profilePage.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;

        profilePageheader.Find("Banner").GetComponent<Image>().sprite = user.profileBanner;
        profilePageheader.Find("ImageMask").Find("Image").GetComponent<Image>().sprite = user.image;
        profilePageheader.Find("Name").GetComponent<TextMeshProUGUI>().text = user.username;
        profilePageheader.Find("Description").GetComponent<TextMeshProUGUI>().text = user.bioText;
        if (pinnedUsers.Contains(user))
        {
            profilePageheader.Find("PinUser").GetComponent<Image>().color = Color.red;
        }
        else
        {
            profilePageheader.Find("PinUser").GetComponent<Image>().color = Color.black;
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

    public void CloseUserProfile()
    {
        currentUser = null;
        profilePage.SetActive(false);
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
            profilePageheader.Find("PinUser").GetComponent<Image>().color = Color.black;
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
}

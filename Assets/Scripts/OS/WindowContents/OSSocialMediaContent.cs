using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Experimental.GraphView;
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

    private int postNumber = 1;
    private GameManager gm;
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
        gm = GameManager.instance;
        foreach (SocialMediaPost s in gm.GetPosts())
        {
            InstanciatePost(s);
        }

        fpsController.OnPinDeletion.AddListener(RemovePinnedUser);
    }

    private void OnEnable()
    {
        if (computerControls.investigationState == OSInvestigationState.PERSON_ACCUSED)
        {
            ClearDeletedPost();
        }
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

    public void InstanciatePost(SocialMediaPost post)
    {
        GameObject newPost = Instantiate(postPrefab, socialMediaPostContainer.transform);
        newPost.GetComponent<OSSocialMediaPost>().instanctiatePost(post);
        newPost.name = "Post" + postNumber;
        postNumber++;
        newPost.transform.Find("name").GetComponent<TextMeshProUGUI>().text = post.author.username;
        newPost.transform.Find("content").GetComponent<TextMeshProUGUI>().text = post.content;
        Instantiate(newPost, profilePageContent.transform);
        postList.Add(newPost.GetComponent<OSSocialMediaPost>());
    }

    public void ClearDeletedPost()
    {
        OnDeletedPostClear?.Invoke();

        /*foreach (OSSocialMediaPost p in postList)
        {
            p.gameObject.transform.Find("PostOptions").Find("DeletePost").GetComponent<Image>().color = Color.black;
        }*/
    }

    public void ShowUserProfile(SocialMediaUser user)
    {
        currentUser = user;
        profilePage.SetActive(true);
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
    }

    public void CloseUserProfile()
    {
        currentUser = null;
        profilePage.SetActive(false);
    }

    public void PinUser()
    {
        OnPinned?.Invoke(currentUser);
    }

    private void RemovePinnedUser(ScriptableObject so)
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OSSocialMediaContent : MonoBehaviour
{
    [SerializeField] private GameObject socialMediaPostContainer;
    [SerializeField] private GameObject postPrefab;
    [SerializeField] private GameObject profilePage;
    [SerializeField] private Transform profilePageheader;
    private int postNumber = 1;
    private GameManager gm;
    private List<OSSocialMediaPost> postList = new List<OSSocialMediaPost>();
    private ComputerControls computerControls;
    private Pinboard pinboard;
    private OSPopupManager popupManager;
    private SocialMediaUser currentUser;
    private List<SocialMediaUser> pinnedUsers = new List<SocialMediaUser>();
    private FPSController fpsController;

    // TODO: remove
    public ScriptableObject userToDelete;

    // Start is called before the first frame update
    void Awake()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
    }

    private void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();
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

    // TODO: remove
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            RemovePinnedUser(userToDelete);
        }
    }

    public void InstanciatePost(SocialMediaPost post)
    {
        GameObject newPost = Instantiate(postPrefab, socialMediaPostContainer.transform);
        newPost.GetComponent<OSSocialMediaPost>().instanctiatePost(post);
        //newPost.GetComponent<Button>().interactable = false;
        //newPost.GetComponentInChildren<Button>().interactable = false;
        newPost.name = "Post" + postNumber;
        postNumber++;
        newPost.transform.Find("name").GetComponent<TextMeshProUGUI>().text = post.author.username;
        newPost.transform.Find("content").GetComponent<TextMeshProUGUI>().text = post.content;
        postList.Add(newPost.GetComponent<OSSocialMediaPost>());
    }

    public void ClearDeletedPost()
    {
        foreach (OSSocialMediaPost p in postList)
        {
            p.gameObject.transform.Find("PostOptions").Find("DeletePost").GetComponent<Image>().color = Color.black;
        }
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
    }

    public void CloseUserProfile()
    {
        currentUser = null;
        profilePage.SetActive(false);
    }

    public void PinUser()
    {
        popupManager.DisplayUserPinMessage();
        pinboard.AddPin(currentUser);
        profilePageheader.Find("PinUser").GetComponent<Image>().color = Color.red;
        AddToPinnedUserList(currentUser);
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
        pinnedUsers.Add(user);
    }
}

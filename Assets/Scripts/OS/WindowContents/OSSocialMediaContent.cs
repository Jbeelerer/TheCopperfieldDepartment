using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Awake()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
    }

    private void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();
        gm = GameManager.instance;
        foreach (SocialMediaPost s in gm.GetPosts())
        {
            InstanciatePost(s);
        }
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
        //userPinned = true;
        profilePageheader.Find("PinUser").GetComponent<Image>().color = Color.red;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OSSocialMediaContent : MonoBehaviour
{
    [SerializeField] private GameObject socialMediaPostContainer;
    [SerializeField] private GameObject postPrefab;
    private int postNumber = 1;
    private GameManager gm;
    private List<OSSocialMediaPost> postList = new List<OSSocialMediaPost>();
    private ComputerControls computerControls;

    // Start is called before the first frame update
    void Awake()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
    }

    private void Start()
    {
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
}

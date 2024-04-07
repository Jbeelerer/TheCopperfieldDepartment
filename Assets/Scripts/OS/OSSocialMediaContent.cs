using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSSocialMediaContent : MonoBehaviour
{
    [SerializeField] private GameObject socialMediaPostContainer;
    [SerializeField] private GameObject postPrefab;
    private int postNumber = 1;
    private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        UpdateContent();
    }

    private void UpdateContent()
    {
        foreach (SocialMediaPost s in gm.GetPosts())
        {
            InstanciatePost(s);
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
    }
}

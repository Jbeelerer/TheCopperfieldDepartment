using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSSocialMediaContent : MonoBehaviour
{
    [SerializeField] private GameObject socialMediaPostContainer;
    [SerializeField] private GameObject postPrefab;
    private int postNumber = 1;

    // Start is called before the first frame update
    void Start()
    {
        SocialMediaPost[] posts = Resources.LoadAll<SocialMediaPost>("Posts");
        foreach (SocialMediaPost s in posts)
        {
            InstanciatePost(s);
            //print(s.content);
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

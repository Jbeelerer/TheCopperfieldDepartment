using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class SocialMediaManager : MonoBehaviour
{

    //[SerializeField] private SocialMediaPost[] socialMediaPosts;
    private SocialMediaPost[] sortedsocialMediaPosts;

    [SerializeField] private GameObject socialMediaPostContainer;

    [SerializeField] private GameObject postPrefab;

    // This will be appended to the name of the created entities and increment when each is created.
    int instanceNumber = 1;

    private bool pinboardAdditionMode = false;

    private void Update()
    {
        if (pinboardAdditionMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Add a new post to the pinboard
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.tag == "name")
                    {
                        print("Hit the name");
                    }
                    else if (hit.transform.tag == "content")
                    {
                        print("Hit the content");
                    }
                }
            }
        }
    }
    void Start()
    {
        // Scriptable is a class of your ScriptableObject
        SocialMediaPost[] scripts = Resources.LoadAll<SocialMediaPost>("Posts");

        foreach (SocialMediaPost s in scripts)
        {
            instanctiatePost(s);
            print(s.content);
        }
    }

    public void instanctiatePost(SocialMediaPost post)
    {
        GameObject newPost = Instantiate(postPrefab, socialMediaPostContainer.transform);
        newPost.GetComponent<SocialMediaPostManager>().instanctiatePost(post);
        newPost.GetComponent<Button>().interactable = false;
        newPost.GetComponentInChildren<Button>().interactable = false;
        newPost.name = "Post" + instanceNumber;
        instanceNumber++;
        newPost.transform.Find("name").GetComponent<TextMeshProUGUI>().text = post.author.username;
        newPost.transform.Find("content").GetComponent<TextMeshProUGUI>().text = post.content;
    }

    public void enableAdditionMode()
    {

        pinboardAdditionMode = !pinboardAdditionMode;
        foreach (Transform child in socialMediaPostContainer.transform)
        {
            child.GetComponent<Button>().interactable = pinboardAdditionMode;
            child.GetComponentInChildren<Button>().interactable = pinboardAdditionMode;
        }
    }

}

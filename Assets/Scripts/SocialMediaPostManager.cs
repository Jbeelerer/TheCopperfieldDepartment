using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SocialMediaPostManager : MonoBehaviour
{
    public SocialMediaPost post;

    private Pinboard pinboard;
    // Start is called before the first frame update
    void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
    }

    // Update is called once per frame
    void Update()
    {

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
                pinboard.AddPin(post.author);
                break;
            case "content":
                pinboard.AddPin(post.author);
                pinboard.AddPin(post);
                break;
            case "person":
                pinboard.AddPin("Person");
                break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashedPostIt : MonoBehaviour
{
    private ScriptableObject content;
    private Pinboard pinboard;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    public ScriptableObject GetContent()
    {
        return content;
    }
    public void SetContent(ScriptableObject content)
    {
        pinboard = FindObjectOfType<Pinboard>();
        this.content = content; 
        pinboard.AddTrashedPin(content, gameObject);
    }
    public void ReAddPostIt()
    {
        pinboard.AddPin(content);
        OSSocialMediaContent oSSocialMediaContent = Object.FindObjectOfType<OSSocialMediaContent>();
        // @alex help??
        if (content.GetType() == typeof(SocialMediaPost))
        {
        }
        else if (content.GetType() == typeof(SocialMediaUser))
        {

        }
        else if (content.GetType() == typeof(Person))
        {

        } 
        Destroy(gameObject);
    }
}

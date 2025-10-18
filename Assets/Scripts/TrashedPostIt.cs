using UnityEngine;

public class TrashedPostIt : MonoBehaviour
{
    private ScriptableObject content;
    private Pinboard pinboard;
    private OSSocialMediaContent socialMediaContent;
    private OSPeopleListContent peopleListContent;
    // Start is called before the first frame update
    void Start()
    {
        socialMediaContent = FindObjectOfType<OSSocialMediaContent>();
        peopleListContent = FindObjectOfType<OSPeopleListContent>();
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
        if (content.GetType() == typeof(SocialMediaPost))
        {
            socialMediaContent.OnPinned?.Invoke((SocialMediaPost)content);
        }
        else if (content.GetType() == typeof(SocialMediaUser))
        {
            socialMediaContent.OnPinned?.Invoke((SocialMediaUser)content);
        }
        else if (content.GetType() == typeof(Person))
        {
            peopleListContent.PinPerson((Person)content);
        } 
        Destroy(gameObject);
    }
}

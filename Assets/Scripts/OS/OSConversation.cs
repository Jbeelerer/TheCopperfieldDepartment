using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSConversation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public DMConversation conversation;

    private GameObject contactOptions;
    private GameManager gm;
    private OSDmPageContent dmPageContent;
    private ComputerControls computerControls;
    private OSSocialMediaContent socialMediaContent;

    private SocialMediaUser sender;
    private SocialMediaUser recipient;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        contactOptions = transform.Find("ContactOptions").gameObject;
        dmPageContent = transform.GetComponentInParent<OSDmPageContent>();
        computerControls = transform.GetComponentInParent<ComputerControls>();
        socialMediaContent = GameObject.Find("DesktopInterface").GetComponentInChildren<OSSocialMediaContent>();
    }

    public void InstantiateConversation(DMConversation convo)
    {
        this.conversation = convo;
    }

    public void MatchConvoToSenderView(SocialMediaUser currentUser)
    {
        if (currentUser == conversation.conversationMember1)
        {
            sender = conversation.conversationMember1;
            recipient = conversation.conversationMember2;
        }
        else
        {
            sender = conversation.conversationMember2;
            recipient = conversation.conversationMember1;
        }
        transform.Find("ContactInfo").Find("Name").GetComponent<TextMeshProUGUI>().text = recipient.username;
        // transform.Find("ContactInfo").Find("ProfilePic").Find("ImageMask").Find("Image").GetComponent<Image>().sprite = recipient.image;
    }

    public void OpenContactProfile()
    {
        computerControls.OpenWindow(OSAppType.SOCIAL);
        socialMediaContent.ShowUserProfile(recipient);
    }

    public void OpenConversation()
    {
        dmPageContent.ShowUserDM(conversation);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        /*foreach (Transform option in contactOptions.transform)
        {
            option.gameObject.SetActive(true);
        }*/
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (Transform option in contactOptions.transform)
        {
            option.gameObject.SetActive(false);

            /*if ((option.name == "PinUser" && userPinned))
            {
                option.gameObject.SetActive(true);
            }*/
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OSDmPageContent : MonoBehaviour
{
    [SerializeField] private GameObject conversationPrefab;
    [SerializeField] private GameObject dmPrefab;
    [SerializeField] private GameObject conversationsPage;
    [SerializeField] private GameObject conversationsPageContent;
    [SerializeField] private GameObject dmPage;
    [SerializeField] private GameObject dmPageContent;
    [SerializeField] private Transform infoBar;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button loginButton;
    [SerializeField] private GameObject loginFailedMessage;
    [SerializeField] private UnityEngine.Color dmNameColor;

    private ComputerControls computerControls;
    private OSPopupManager popupManager;

    private SocialMediaUser currentUser;
    private bool passwordFound = false;

    void Awake()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
    }

    private void Start()
    {
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();

        ChangeInfoBar("Login Page", hidden: true);

        usernameField.text = currentUser.username;
        if (passwordFound)
        {
            InstantiateConversations();
            passwordField.text = "********";
        }
        loginButton.interactable = passwordFound;
        loginFailedMessage.SetActive(!passwordFound);
    }

    public void InitializeLoginPage(SocialMediaUser user, bool pwFound)
    {
        currentUser = user;
        passwordFound = pwFound;
    }

    private void InstantiateConversations()
    {
        foreach (DMConversation convo in computerControls.GetConversations())
        {
            if (convo.conversationMember1 == currentUser || convo.conversationMember2 == currentUser)
            {
                GameObject newConvo = Instantiate(conversationPrefab, conversationsPageContent.transform);
                newConvo.GetComponent<OSConversation>().InstantiateConversation(convo);
                newConvo.name = "Convo" + convo.id;
                newConvo.GetComponent<OSConversation>().MatchConvoToSenderView(currentUser);
            }
        }
    }

    public void ShowConversationsPage()
    {
        ChangeInfoBar("Messages");

        conversationsPage.transform.SetAsLastSibling();
        conversationsPage.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    }

    public void ShowUserDM(DMConversation convo)
    {
        SocialMediaUser recipient = convo.conversationMember1 == currentUser ? convo.conversationMember2 : convo.conversationMember1;

        ChangeInfoBar(recipient.username, ShowConversationsPage);

        foreach (Transform dm in dmPageContent.transform)
        {
            Destroy(dm.gameObject);
        }
        convo.messages.ForEach(m =>
        {
            GameObject newDM = Instantiate(dmPrefab, dmPageContent.transform);

            string message = "<b>";
            message += (m.sender == currentUser ? "You" : "<color=#" + UnityEngine.ColorUtility.ToHtmlStringRGBA(dmNameColor) + ">" + recipient.username) + " ";
            message += "(" + m.timeStamp + "):</color></b> ";
            message += m.message;

            newDM.GetComponentInChildren<TextMeshProUGUI>().text = message;
            newDM.name = "DM";
        });

        dmPage.transform.SetAsLastSibling();
    }

    private void ChangeInfoBar(string text, UnityAction backButtonFunc = null, bool hidden = false)
    {
        infoBar.gameObject.SetActive(!hidden);
        if (hidden) return;
        Debug.Log("Test");

        infoBar.Find("Title").GetComponent<TextMeshProUGUI>().text = text;
        infoBar.Find("BackButton").gameObject.SetActive(backButtonFunc != null);
        if (backButtonFunc != null)
        {
            infoBar.Find("BackButton").GetComponent<Button>().onClick.RemoveAllListeners();
            infoBar.Find("BackButton").GetComponent<Button>().onClick.AddListener(() => backButtonFunc());
        }
    }

    public void ShowLoginPopup()
    {
        popupManager.DisplayAccountLoginMessage();
    }
}

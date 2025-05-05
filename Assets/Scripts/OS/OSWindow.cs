using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public enum WindowSize
{
    SMALL,
    LONG_LEFT,
    LONG_RIGHT,
    BIG
}

public class OSWindow : MonoBehaviour
{
    [HideInInspector] public RectTransform rectTrans;
    public OSAppType appType;
    [HideInInspector] public RectTransform topBar;
    [HideInInspector] public RectTransform buttonSmall;
    [HideInInspector] public RectTransform buttonLong;
    [HideInInspector] public RectTransform buttonBig;
    [HideInInspector] public RectTransform buttonClose;
    [HideInInspector] public RectTransform[] resizeButtons = new RectTransform[] { };

    [HideInInspector] public RectTransform sideswapLeft;
    [HideInInspector] public RectTransform sideswapRight;
    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public bool canBeMoved = true;
    [HideInInspector] public WindowSize currWindowSize = WindowSize.SMALL;
    [HideInInspector] public OSTab associatedTab;
    [HideInInspector] public GameObject content = null;
    [HideInInspector] public string warningMessage = "";
    [HideInInspector] public System.Action warningSuccessFunc = null;
    [HideInInspector] public bool hasCancelBtn = true;
    [HideInInspector] public SocialMediaPost imagePost = null;
    [HideInInspector] public SocialMediaUser dmUser = null;
    [HideInInspector] public bool dmUserPasswordFound = false;
    [HideInInspector] public bool multipleInstancesAllowed = false;

    [SerializeField] private Sprite customSocialMediaWindow;
    [SerializeField] private Sprite customSocialMediaTopBar;

    [SerializeField] private GameObject socialMediaContent;
    [SerializeField] private GameObject govAppContent;
    [SerializeField] private GameObject settingsContent;
    [SerializeField] private GameObject peopleListContent;
    [SerializeField] private GameObject warningContent;
    [SerializeField] private GameObject startSettingsContent;
    [SerializeField] private GameObject bigImageContent;
    [SerializeField] private GameObject dmPageContent;

    private void Awake()
    {
        rectTrans = GetComponent<RectTransform>();
        topBar = transform.Find("TopBar").GetComponent<RectTransform>();

        buttonSmall = transform.Find("TopBar").Find("Buttons").Find("ButtonSmall").GetComponent<RectTransform>();
        buttonLong = transform.Find("TopBar").Find("Buttons").Find("ButtonLong").GetComponent<RectTransform>();
        buttonBig = transform.Find("TopBar").Find("Buttons").Find("ButtonBig").GetComponent<RectTransform>();
        buttonClose = transform.Find("TopBar").Find("Buttons").Find("ButtonClose").GetComponent<RectTransform>();

        sideswapLeft = transform.Find("SideswapLeft").GetComponent<RectTransform>();
        sideswapRight = transform.Find("SideswapRight").GetComponent<RectTransform>();
    }

    void Start()
    {
        TextMeshProUGUI topBarTextMesh = transform.Find("TopBar").Find("Text").GetComponent<TextMeshProUGUI>();//.text = appType.ToString();

        if (appType == OSAppType.TEST)
        {
            topBarTextMesh.text = "Test Window";
        }
        else if (appType == OSAppType.SOCIAL)
        {
            content = Instantiate(socialMediaContent, transform.Find("Content"));
            resizeButtons = new RectTransform[] { buttonSmall, buttonLong };
            topBarTextMesh.text = "QWAKR";
            GetComponent<Image>().sprite = customSocialMediaWindow;
            topBar.GetComponent<Image>().sprite = customSocialMediaTopBar;
        }
        else if (appType == OSAppType.GOV)
        {
            content = Instantiate(govAppContent, transform.Find("Content"));
            topBarTextMesh.text = "Inbox";
        }
        else if (appType == OSAppType.SETTINGS)
        {
            content = Instantiate(settingsContent, transform.Find("Content"));
            resizeButtons = new RectTransform[] { buttonSmall, buttonLong };
            topBarTextMesh.text = "Computer Settings";
        }
        else if (appType == OSAppType.PEOPLE_LIST)
        {
            content = Instantiate(peopleListContent, transform.Find("Content"));
            resizeButtons = new RectTransform[] { buttonSmall, buttonLong };
            topBarTextMesh.text = "List of Suspects";
        }
        else if (appType == OSAppType.WARNING)
        {
            content = Instantiate(warningContent, transform.Find("Content"));
            content.GetComponent<OSWarningContent>().SetWarningMessage(warningMessage);
            content.GetComponent<OSWarningContent>().SetWarningSuccessFunc(warningSuccessFunc);
            content.transform.Find("ButtonCancel").gameObject.SetActive(hasCancelBtn);
            buttonClose.gameObject.SetActive(false);
            canBeMoved = false;
            topBarTextMesh.text = "Warning";
        }
        else if (appType == OSAppType.START_SETTINGS)
        {
            content = Instantiate(startSettingsContent, transform.Find("Content"));
            buttonClose.gameObject.SetActive(false);
            canBeMoved = false;
            topBarTextMesh.text = "Start Settings";
        }
        else if (appType == OSAppType.IMAGE)
        {
            content = Instantiate(bigImageContent, transform.Find("Content"));
            content.GetComponent<OSBigImageContent>().Setup(imagePost);
            resizeButtons = new RectTransform[] { buttonSmall, buttonLong, buttonBig };
            multipleInstancesAllowed = true;
            topBarTextMesh.text = "Image Viewer";
        }
        else if (appType == OSAppType.DM_PAGE)
        {
            content = Instantiate(dmPageContent, transform.Find("Content"));
            content.GetComponent<OSDmPageContent>().InitializeLoginPage(dmUser, dmUserPasswordFound);
            resizeButtons = new RectTransform[] { buttonSmall, buttonLong };
            multipleInstancesAllowed = true;
            topBarTextMesh.text = "Direct Messages";
        }

        // Activate allowed resize buttons (except for buttonSmall, it will be deactivated either way)
        buttonLong.gameObject.SetActive(false);
        buttonBig.gameObject.SetActive(false);
        foreach (RectTransform resizeButton in resizeButtons)
        {
            resizeButton.gameObject.SetActive(true);
        }
        buttonSmall.gameObject.SetActive(false);
    }

    public void MoveWindow(Vector2 moveVec)
    {
        rectTrans.anchoredPosition += moveVec;
    }
}

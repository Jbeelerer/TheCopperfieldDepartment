using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [HideInInspector] public RectTransform sideswapLeft;
    [HideInInspector] public RectTransform sideswapRight;
    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public bool canBeMoved = true;
    [HideInInspector] public WindowSize currWindowSize = WindowSize.SMALL;
    [HideInInspector] public OSTab associatedTab;
    [HideInInspector] public string warningMessage = "";
    [HideInInspector] public System.Action warningSuccessFunc = null;
    [HideInInspector] public bool hasCancelBtn = true;

    [SerializeField] private Sprite customSocialMediaWindow;
    [SerializeField] private Sprite customSocialMediaTopBar;

    [SerializeField] private GameObject socialMediaContent;
    [SerializeField] private GameObject govAppContent;
    [SerializeField] private GameObject settingsContent;
    [SerializeField] private GameObject peopleListContent;
    [SerializeField] private GameObject warningContent;
    [SerializeField] private GameObject startSettingsContent;

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

        // Deactivate buttonBig for every window, not sure if even needed anymore
        buttonBig.gameObject.SetActive(false);

        if (appType == OSAppType.TEST)
        {
            buttonSmall.gameObject.SetActive(false);
            topBarTextMesh.text = "Test Window";
        }
        else if (appType == OSAppType.SOCIAL)
        {
            Instantiate(socialMediaContent, transform.Find("Content"));
            buttonSmall.gameObject.SetActive(false);
            topBarTextMesh.text = "QWAKR";
            GetComponent<Image>().sprite = customSocialMediaWindow;
            topBar.GetComponent<Image>().sprite = customSocialMediaTopBar;
        }
        else if (appType == OSAppType.GOV)
        {
            Instantiate(govAppContent, transform.Find("Content"));
            buttonBig.gameObject.SetActive(false);
            buttonLong.gameObject.SetActive(false);
            buttonSmall.gameObject.SetActive(false);
            topBarTextMesh.text = "Inbox";
        }
        else if (appType == OSAppType.SETTINGS)
        {
            Instantiate(settingsContent, transform.Find("Content"));
            buttonSmall.gameObject.SetActive(false);
            topBarTextMesh.text = "Computer Settings";
        }
        else if (appType == OSAppType.PEOPLE_LIST)
        {
            Instantiate(peopleListContent, transform.Find("Content"));
            buttonSmall.gameObject.SetActive(false);
            topBarTextMesh.text = "List of Suspects";
        }
        else if (appType == OSAppType.WARNING)
        {
            GameObject content = Instantiate(warningContent, transform.Find("Content"));
            content.GetComponent<OSWarningContent>().SetWarningMessage(warningMessage);
            content.GetComponent<OSWarningContent>().SetWarningSuccessFunc(warningSuccessFunc);
            content.transform.Find("ButtonCancel").gameObject.SetActive(hasCancelBtn);
            buttonClose.gameObject.SetActive(false);
            buttonBig.gameObject.SetActive(false);
            buttonLong.gameObject.SetActive(false);
            buttonSmall.gameObject.SetActive(false);
            canBeMoved = false;
            topBarTextMesh.text = "Warning";
        }
        else if (appType == OSAppType.START_SETTINGS)
        {
            GameObject content = Instantiate(startSettingsContent, transform.Find("Content"));
            buttonClose.gameObject.SetActive(false);
            buttonBig.gameObject.SetActive(false);
            buttonLong.gameObject.SetActive(false);
            buttonSmall.gameObject.SetActive(false);
            canBeMoved = false;
            topBarTextMesh.text = "Start Settings";
        }
    }

    public void MoveWindow(Vector2 moveVec)
    {
        rectTrans.anchoredPosition += moveVec;
    }
}

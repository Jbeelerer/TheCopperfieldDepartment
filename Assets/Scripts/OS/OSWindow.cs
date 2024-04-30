using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [SerializeField] private GameObject socialMediaContent;
    [SerializeField] private GameObject govAppContent;
    [SerializeField] private GameObject peopleListContent;
    [SerializeField] private GameObject warningContent;

    private void Awake()
    {
        rectTrans = GetComponent<RectTransform>();
        topBar = transform.Find("TopBar").GetComponent<RectTransform>();

        sideswapLeft = transform.Find("SideswapLeft").GetComponent<RectTransform>();
        sideswapRight = transform.Find("SideswapRight").GetComponent<RectTransform>();
    }

    void Start()
    {
        buttonSmall = transform.Find("TopBar").Find("Buttons").Find("ButtonSmall").GetComponent<RectTransform>();
        buttonLong = transform.Find("TopBar").Find("Buttons").Find("ButtonLong").GetComponent<RectTransform>();
        buttonBig = transform.Find("TopBar").Find("Buttons").Find("ButtonBig").GetComponent<RectTransform>();
        buttonClose = transform.Find("TopBar").Find("Buttons").Find("ButtonClose").GetComponent<RectTransform>();
        
        transform.Find("TopBar").Find("Text").GetComponent<TextMeshProUGUI>().text = appType.ToString();

        // deactivate buttonBig for every window, not sure if even needed anymore
        buttonBig.gameObject.SetActive(false);

        if (appType == OSAppType.TEST)
        {
            buttonSmall.gameObject.SetActive(false);
        }
        else if (appType == OSAppType.SOCIAL)
        {
            Instantiate(socialMediaContent, transform.Find("Content"));
            buttonSmall.gameObject.SetActive(false);
        }
        else if (appType == OSAppType.GOV)
        {
            Instantiate(govAppContent, transform.Find("Content"));
            buttonBig.gameObject.SetActive(false);
            buttonLong.gameObject.SetActive(false);
            buttonSmall.gameObject.SetActive(false);
        }
        else if (appType == OSAppType.PEOPLE_LIST)
        {
            Instantiate(peopleListContent, transform.Find("Content"));
            buttonSmall.gameObject.SetActive(false);
        }
        else if (appType == OSAppType.WARNING)
        {
            GameObject content = Instantiate(warningContent, transform.Find("Content"));
            content.GetComponent<OSWarningContent>().SetWarningMessage(warningMessage);
            content.GetComponent<OSWarningContent>().SetWarningSuccessFunc(warningSuccessFunc);
            buttonClose.gameObject.SetActive(false);
            buttonBig.gameObject.SetActive(false);
            buttonLong.gameObject.SetActive(false);
            buttonSmall.gameObject.SetActive(false);
            canBeMoved = false;
        }
    }

    public void MoveWindow(Vector2 moveVec)
    {
        rectTrans.anchoredPosition += moveVec;
    }
}

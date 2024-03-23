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
    [HideInInspector] public OSAppType appType;
    [HideInInspector] public RectTransform topBar;
    [HideInInspector] public RectTransform buttonSmall;
    [HideInInspector] public RectTransform buttonLong;
    [HideInInspector] public RectTransform buttonBig;
    [HideInInspector] public RectTransform buttonClose;
    [HideInInspector] public RectTransform sideswapLeft;
    [HideInInspector] public RectTransform sideswapRight;
    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public WindowSize currWindowSize = WindowSize.SMALL;

    // Start is called before the first frame update
    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
        topBar = transform.Find("TopBar").GetComponent<RectTransform>();
        buttonSmall = transform.Find("TopBar").Find("ButtonSmall").GetComponent<RectTransform>();
        buttonLong = transform.Find("TopBar").Find("ButtonLong").GetComponent<RectTransform>();
        buttonBig = transform.Find("TopBar").Find("ButtonBig").GetComponent<RectTransform>();
        buttonClose = transform.Find("TopBar").Find("ButtonClose").GetComponent<RectTransform>();
        sideswapLeft = transform.Find("SideswapLeft").GetComponent<RectTransform>();
        sideswapRight = transform.Find("SideswapRight").GetComponent<RectTransform>();
        transform.Find("Content").Find("Text").GetComponent<TextMeshProUGUI>().text = appType.ToString();
    }

    public void MoveWindow(Vector2 moveVec)
    {
        rectTrans.anchoredPosition += moveVec;
    }
}

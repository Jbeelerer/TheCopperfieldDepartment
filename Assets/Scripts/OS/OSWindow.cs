using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSWindow : MonoBehaviour
{
    [HideInInspector] public RectTransform rectTrans;
    [HideInInspector] public OSAppType appType;
    [HideInInspector] public RectTransform topBar;
    [HideInInspector] public RectTransform buttonRight;
    [HideInInspector] public bool isMoving = false;

    // Start is called before the first frame update
    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
        topBar = transform.Find("TopBar").GetComponent<RectTransform>();
        buttonRight = transform.Find("TopBar").Find("ButtonRight").GetComponent<RectTransform>();
        transform.Find("Content").Find("Text").GetComponent<TextMeshProUGUI>().text = appType.ToString();
    }

    public void MoveWindow(Vector2 moveVec)
    {
        rectTrans.anchoredPosition += moveVec;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSWindow : MonoBehaviour
{
    private RectTransform window;

    [HideInInspector] public RectTransform topBar;
    public bool isMoving = false;

    // Start is called before the first frame update
    void Start()
    {
        window = GetComponent<RectTransform>();
        topBar = transform.Find("TopBar").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    public void MoveWindow(Vector2 moveVec)
    {
        window.anchoredPosition += moveVec;
    }
}

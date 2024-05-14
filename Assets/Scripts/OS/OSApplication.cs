using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OSApplication : MonoBehaviour
{
    public OSAppType appType;

    [HideInInspector] public Image appIcon;
    [HideInInspector] public RectTransform recTrans;

    // Start is called before the first frame update
    void Start()
    {
        appIcon = GetComponent<Image>();
        recTrans = GetComponent<RectTransform>();
    }
}

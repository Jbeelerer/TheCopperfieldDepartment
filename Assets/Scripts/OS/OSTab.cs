using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSTab : MonoBehaviour
{
    public OSAppType appType;

    [HideInInspector] public RectTransform recTrans;

    // Start is called before the first frame update
    void Start()
    {
        recTrans = GetComponent<RectTransform>();
        transform.Find("Text").GetComponent<TextMeshProUGUI>().text = appType.ToString();
    }
}

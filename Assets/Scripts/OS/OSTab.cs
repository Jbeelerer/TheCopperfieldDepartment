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
        switch(appType)
        {
            case OSAppType.SOCIAL:
                transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Quakr";
                break;
            case OSAppType.GOV:
                transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Inbox";
                break;
            case OSAppType.PEOPLE_LIST:
                transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Suspects";
                break;
            case OSAppType.SETTINGS:
                transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Settings";
                break;
            case OSAppType.WARNING:
                transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Warning";
                break;
            case OSAppType.START_SETTINGS:
                transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Settings";
                break;
            default:
                transform.Find("Text").GetComponent<TextMeshProUGUI>().text = appType.ToString();
                break;
        }
    }
}

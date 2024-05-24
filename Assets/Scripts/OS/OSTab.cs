using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OSTab : MonoBehaviour
{
    public OSAppType appType;

    [HideInInspector] public RectTransform recTrans;

    private ComputerControls computerControls;

    // Start is called before the first frame update
    void Start()
    {
        recTrans = GetComponent<RectTransform>();
        computerControls = transform.GetComponentInParent<ComputerControls>();

        transform.Find("GameObject").GetComponent<Image>().sprite = computerControls.appIcons[(int)appType];

        string tabText = "New";
        switch (appType)
        {
            case OSAppType.SOCIAL:
                tabText = "QWAKR";
                break;
            case OSAppType.GOV:
                tabText = "Inbox";
                break;
            case OSAppType.WARNING:
                tabText = "Warning";
                break;
            case OSAppType.START_SETTINGS:
                tabText = "Settings";
                break;
                case OSAppType.SETTINGS:
                tabText = "Settings";
                break;
            case OSAppType.PEOPLE_LIST: 
                tabText = "Suspects";
                break;
        }
        transform.Find("Text").GetComponent<TextMeshProUGUI>().text = tabText;
    }
}

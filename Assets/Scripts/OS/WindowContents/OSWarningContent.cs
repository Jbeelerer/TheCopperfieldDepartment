using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSWarningContent : MonoBehaviour
{
    private TextMeshProUGUI warningMessage;
    private System.Action warningSuccessFunc;
    private GameObject screenBlockadeBG;
    private GameObject screenBlockadeTaskBar;

    void Awake()
    {
        warningMessage = transform.Find("Message").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        screenBlockadeBG = GameObject.Find("ScreenBlockadeBG");
        screenBlockadeBG.GetComponent<Image>().enabled = true;
        screenBlockadeBG.transform.SetAsLastSibling();

        screenBlockadeTaskBar = GameObject.Find("ScreenBlockadeTaskBar");
        screenBlockadeTaskBar.GetComponent<Image>().enabled = true;

        transform.GetComponentInParent<OSWindow>().transform.SetAsLastSibling();
    }

    public void ConfirmWarning()
    {
        if (warningSuccessFunc != null)
            warningSuccessFunc.Invoke();

        CloseWarning();
    }

    public void CancelWarning()
    {
        CloseWarning();
    }

    private void CloseWarning()
    {
        screenBlockadeBG.GetComponent<Image>().enabled = false;
        screenBlockadeTaskBar.GetComponent<Image>().enabled = false;
        Destroy(GetComponentInParent<OSWindow>().associatedTab.gameObject);
        Destroy(GetComponentInParent<OSWindow>().gameObject);
    }

    public void SetWarningMessage(string msg)
    {
        warningMessage.text = msg;
    }

    public void SetWarningSuccessFunc(System.Action func)
    {
        warningSuccessFunc = func;
    }
}

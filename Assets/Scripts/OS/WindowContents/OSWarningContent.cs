using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSWarningContent : MonoBehaviour
{
    private ComputerControls computerControls;
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
        computerControls = transform.GetComponentInParent<ComputerControls>();

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

        computerControls.CloseWindow(GetComponentInParent<OSWindow>());
    }

    public void CancelWarning()
    {
        computerControls.CloseWindow(GetComponentInParent<OSWindow>());
    }

    public void HideScreenBlockade()
    {
        screenBlockadeBG.GetComponent<Image>().enabled = false;
        screenBlockadeTaskBar.GetComponent<Image>().enabled = false;
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

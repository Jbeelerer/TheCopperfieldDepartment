using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSGovAppContent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    public TMP_Text mailTextMesh;
    public TMP_Text mailTitle;
    public TMP_Text mailSender;
    public GameObject mailContainer;
    public GameObject mailPrefab;
    public ScrollRect textScrollArea;

    private Camera canvasCam;
    private ComputerControls computerControls;
    private Mail[] mails;

    void Start()
    {
        canvasCam = GameObject.Find("computerTextureCam").GetComponent<Camera>();
        computerControls = GetComponentInParent<ComputerControls>();

        mails = computerControls.GetMails();
        foreach (Mail m in mails)
        {
            GameObject newMail = Instantiate(mailPrefab, mailContainer.transform);
            newMail.GetComponent<OSMail>().mail = m;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(mailTextMesh, eventData.position, canvasCam);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = mailTextMesh.textInfo.linkInfo[linkIndex];
            if (linkInfo.GetLinkID() == "VigilantyVirus")
            {
                computerControls.OpenWindow(OSAppType.WARNING, "WizOS has detected harmful software on the system.<br><br><b>Please contact an administrator.</b>", OpenVigilanty, false);
            }
            else
            {
                computerControls.OpenWindow(OSAppType.PEOPLE_LIST);
            }
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(mailTextMesh, eventData.position, canvasCam);
        if (computerControls.GetFirstHitObject() && !computerControls.GetFirstHitObject().GetComponent<Button>())
        {
            computerControls.cursor.GetComponent<Image>().sprite = linkIndex != -1 ? computerControls.cursorClickable : computerControls.cursorNormal;
        }
    }

    private void OpenVigilanty()
    {
        computerControls.pointySystem.StartTutorial("EvilIntro");
        computerControls.pointySystem.evilIntroCompleted = true;

        if (mailContainer.transform.childCount == 1)
        {
            GameObject newMail = Instantiate(mailPrefab, mailContainer.transform);
            newMail.GetComponent<OSMail>().mail = mails[1];
            newMail = Instantiate(mailPrefab, mailContainer.transform);
            newMail.GetComponent<OSMail>().mail = mails[2];
        }
    }
}

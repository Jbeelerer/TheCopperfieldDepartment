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
    public RectTransform mailListRect;
    public RectTransform textBoxRect;
    public Mail currentMail;

    private Camera canvasCam;
    private ComputerControls computerControls;
    private List<Mail> mails = new List<Mail>();

    void Start()
    {
        canvasCam = GameObject.Find("computerTextureCam").GetComponent<Camera>();
        computerControls = GetComponentInParent<ComputerControls>();

        for (int i = 1; i <= GameManager.instance.GetDay(); i++)
        {
            var retreivedMails = computerControls.GetMails(i);
            foreach (Mail m in retreivedMails)
            {
                GameObject newMail = Instantiate(mailPrefab, mailContainer.transform);
                newMail.GetComponent<OSMail>().Instantiate(m, i);
            }
            mails.AddRange(retreivedMails);
        }

        CloseTextBox();
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
                computerControls.OpenWindow(OSAppType.PEOPLE_LIST, peopleListDay: currentMail.day);

                // Show pin tutorial on day 2 only if the current people list is opened
                if (GameManager.instance.GetDay() == 2 && currentMail.day == GameManager.instance.GetDay())
                {
                    computerControls.pointySystem.StartTutorial("PeopleListPinning");
                }
            }
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(mailTextMesh, eventData.position, canvasCam);
        if (computerControls.GetFirstHitObject() && !computerControls.GetFirstHitObject().GetComponent<Button>())
        {
            computerControls.cursor.GetComponent<Image>().sprite = linkIndex != -1 ? computerControls.cursorClickable : computerControls.cursorNormal;
            computerControls.isHoveringOverLink = linkIndex != -1;
        }
    }

    // Used on the close message button
    public void CloseTextBox()
    {
        mailListRect.offsetMin = new Vector2(0, 35);
        textBoxRect.offsetMax = new Vector2(0, -228);
        textBoxRect.transform.GetChild(0).Find("CloseTextBoxButton").gameObject.SetActive(false);
        textScrollArea.transform.Find("ScrollArea").gameObject.SetActive(false);
    }

    public void OpenTextBox()
    {
        mailListRect.offsetMin = new Vector2(0, 180);
        textBoxRect.offsetMax = new Vector2(0, -82);
        textBoxRect.transform.GetChild(0).Find("CloseTextBoxButton").gameObject.SetActive(true);
        textScrollArea.transform.Find("ScrollArea").gameObject.SetActive(true);
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

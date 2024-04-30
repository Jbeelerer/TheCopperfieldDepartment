using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSGovAppContent : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler
{
    public TMP_Text mailTextMesh;
    public TMP_Text mailTitle;
    public TMP_Text mailSender;
    public GameObject mailContainer;
    public GameObject mailPrefab;

    private Camera canvasCam;
    private ComputerControls computerControls;
    private Mail[] mails;

    void Start()
    {
        canvasCam = GameObject.Find("computerTextureCam").GetComponent<Camera>();
        computerControls = GetComponentInParent<ComputerControls>();

        // TODO: Load mails through GameManager
        mails = Resources.LoadAll<Mail>("Case1/Mails");
        foreach (Mail m in mails)
        {
            GameObject newMail = Instantiate(mailPrefab, mailContainer.transform);
            newMail.GetComponent<OSMail>().mail = m;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(mailTextMesh, eventData.position, canvasCam);
        if (linkIndex != -1)
        {
            computerControls.OpenWindow(OSAppType.PEOPLE_LIST);
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
}

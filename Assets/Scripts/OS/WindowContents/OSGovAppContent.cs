using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OSGovAppContent : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text mailTextMesh;
    private Camera canvasCam;
    private ComputerControls computerControls;

    void Start()
    {
        canvasCam = GameObject.Find("computerTextureCam").GetComponent<Camera>();
        computerControls = GetComponentInParent<ComputerControls>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(mailTextMesh, eventData.position, canvasCam);
        if (linkIndex != -1)
        {
            //TMP_LinkInfo linkInfo = mailTextMesh.textInfo.linkInfo[linkIndex];
            //Application.OpenURL(linkInfo.GetLinkID());
            computerControls.OpenWindow(OSAppType.PEOPLE_LIST);
        }
    }
}

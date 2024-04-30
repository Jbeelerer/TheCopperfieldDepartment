using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSMail : MonoBehaviour
{
    public Mail mail;

    private OSGovAppContent govAppContent;

    void Start()
    {
        govAppContent = transform.GetComponentInParent<OSGovAppContent>();
        transform.Find("Sender").GetComponent<TextMeshProUGUI>().text = mail.sender;
        transform.Find("Title").GetComponent<TextMeshProUGUI>().text = mail.title;
    }

    public void OpenMail()
    {
        govAppContent.mailTitle.text = "<b>" + mail.title + "</b>";
        govAppContent.mailSender.text = "From: " + mail.sender;
        govAppContent.mailTextMesh.text = mail.message;
    }
}

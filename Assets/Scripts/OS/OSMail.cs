using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OSMail : MonoBehaviour
{
    public Mail mail;

    [SerializeField] private Sprite mailOpenedIcon;

    private OSGovAppContent govAppContent;
    private ComputerControls computerControls;
    private bool isUnopened = true;

    void Start()
    {
        govAppContent = transform.GetComponentInParent<OSGovAppContent>();
        computerControls = transform.GetComponentInParent<ComputerControls>();

        transform.Find("Date").GetComponent<TextMeshProUGUI>().text = "<b>" + computerControls.computerDate.text;
        transform.Find("Sender").GetComponent<TextMeshProUGUI>().text = "<b>" + mail.sender;
        transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "<b>" + mail.title;
        transform.Find("MainCaseIcon").gameObject.SetActive(mail.isMainCase);
    }

    public void OpenMail()
    {
        // Remove the bold after clicking on mail once
        if (isUnopened)
        {
            transform.Find("Date").GetComponent<TextMeshProUGUI>().text = transform.Find("Date").GetComponent<TextMeshProUGUI>().text.Substring(3);
            transform.Find("Sender").GetComponent<TextMeshProUGUI>().text = transform.Find("Sender").GetComponent<TextMeshProUGUI>().text.Substring(3);
            transform.Find("Title").GetComponent<TextMeshProUGUI>().text = transform.Find("Title").GetComponent<TextMeshProUGUI>().text.Substring(3);
            transform.Find("MailReadIcon").GetComponent<Image>().sprite = mailOpenedIcon;
            isUnopened = false;
        }

        govAppContent.mailTitle.text = "<b>" + mail.title;
        govAppContent.mailSender.text = "From: " + mail.sender;
        govAppContent.mailTextMesh.text = mail.message;
    }
}

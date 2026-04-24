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

    public void Instantiate(Mail m, int day)
    {
        govAppContent = transform.GetComponentInParent<OSGovAppContent>();
        computerControls = transform.GetComponentInParent<ComputerControls>();

        mail = m;
        mail.day = day;
        transform.Find("Date").GetComponent<TextMeshProUGUI>().text = "<b>" + computerControls.initialComputerDate.AddDays(day - 1).ToString("MM/dd", new System.Globalization.CultureInfo("en-US"));
        transform.Find("Sender").GetComponent<TextMeshProUGUI>().text = "<b>" + mail.sender;
        transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "<b>" + mail.title;
        transform.Find("MainCaseIcon").gameObject.SetActive(mail.isMainCase && day == GameManager.instance.GetDay());
        if (day < GameManager.instance.GetDay())
        {
            RemoveBold();
        }
    }

    public void OpenMail()
    {
        // Remove bold/make text color lighter after clicking on mail
        RemoveBold();

        govAppContent.currentMail = mail;
        govAppContent.mailTitle.text = "<b>" + mail.title;
        govAppContent.mailSender.text = "From: " + mail.sender;
        govAppContent.mailTextMesh.text = mail.message;
        govAppContent.textScrollArea.GetComponentInChildren<Scrollbar>(true).value = 1;
        govAppContent.OpenTextBox();
    }

    private void RemoveBold()
    {
        if (isUnopened)
        {
            transform.Find("Date").GetComponent<TextMeshProUGUI>().text = transform.Find("Date").GetComponent<TextMeshProUGUI>().text.Substring(3);
            transform.Find("Date").GetComponent<TextMeshProUGUI>().color = Color.gray;
            transform.Find("Sender").GetComponent<TextMeshProUGUI>().text = transform.Find("Sender").GetComponent<TextMeshProUGUI>().text.Substring(3);
            transform.Find("Sender").GetComponent<TextMeshProUGUI>().color = Color.gray;
            transform.Find("Title").GetComponent<TextMeshProUGUI>().text = transform.Find("Title").GetComponent<TextMeshProUGUI>().text.Substring(3);
            transform.Find("Title").GetComponent<TextMeshProUGUI>().color = Color.gray;
            transform.Find("MailReadIcon").GetComponent<Image>().sprite = mailOpenedIcon;
        }
        isUnopened = false;
    }
}

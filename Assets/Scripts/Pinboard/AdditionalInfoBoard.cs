using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.Impl;

public class AdditionalInfoBoard : MonoBehaviour
{
    private ScriptableObject content;
    private Animator anim;

    private UnityEngine.UI.Image image;
    private TextMeshProUGUI title;
    private TextMeshProUGUI additionalInfos;

    private TextMeshProUGUI contentText;

    private Transform personParent;

    private Transform contentParent;

    private List<string> newInfo = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        contentParent = transform.Find("Content");
        personParent = transform.Find("Person");

        image = personParent.Find("PB").GetComponent<UnityEngine.UI.Image>();
        title = personParent.Find("Titel").GetComponent<TextMeshProUGUI>();
        additionalInfos = personParent.Find("AdditionalInfos/AdditionalInfosText").GetComponent<TextMeshProUGUI>();

        contentText = contentParent.Find("contentText").GetComponent<TextMeshProUGUI>();


        additionalInfos.text = "";
        anim = GetComponent<Animator>();

    }
    public void ShowInfo(bool b)
    {
        anim.SetBool("showInfo", b);
    }
    public void ShowInfo(bool b, ScriptableObject o)
    {
        if (o != content)
            SetContent(o);
        ShowInfo(b);
    }
    public void StartPreview(ScriptableObject o)
    {
        SetContent(o);
        anim.SetTrigger("previewTrigger");
    }
    public void CancelPreview()
    {
        anim.SetBool("showInfo", false);
        anim.SetTrigger("cancelPreview");
    }

    public void SetContent(ScriptableObject o)
    {
        contentParent.gameObject.SetActive(!(o is Person || o is SocialMediaUser));
        personParent.gameObject.SetActive(o is Person || o is SocialMediaUser);
        additionalInfos.text = "";
        // check if scribtable object is type person  
        switch (o)
        {
            case Person:
                Person p = (Person)o;
                title.text = p.personName;
                image.sprite = p.image;
                foreach (string info in p.additionalInfos)
                {
                    additionalInfos.text += "- " + info + "<br>";
                }
                break;
            case SocialMediaUser:
                SocialMediaUser smu = (SocialMediaUser)o;
                image.sprite = smu.image;
                title.text = smu.username;
                foreach (string info in smu.additionalInfos)
                {
                    additionalInfos.text += "- " + info + "<br>";
                }
                break;
            case SocialMediaPost:
                SocialMediaPost smp = (SocialMediaPost)o;
                contentText.text = smp.content;
                break;
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}

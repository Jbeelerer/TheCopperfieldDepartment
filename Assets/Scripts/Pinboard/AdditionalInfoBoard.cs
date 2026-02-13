using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.Impl;
using Unity.VisualScripting;

public class AdditionalInfoBoard : MonoBehaviour
{
    [SerializeField] private GameObject backgroundClipboardImage; 
    [SerializeField] private Sprite userInfoBG; 
    [SerializeField] private Sprite personInfoBG;
    [SerializeField] private Sprite postContentInfoBG;
    [SerializeField] private Sprite paperBG;

    // user
    [SerializeField] private GameObject userInfo;
    [SerializeField] private Image userImage;
    [SerializeField] private TMP_Text userName; 
    [SerializeField] private TMP_Text userContent;

    // person
    [SerializeField] private GameObject personInfo;
    [SerializeField] private Image personImage;
    [SerializeField] private TMP_Text personName;
    [SerializeField] private TMP_Text personContent;

    // post content
    [SerializeField] private GameObject postContentInfo;
    [SerializeField] private GameObject postContentUserImage;
    [SerializeField] private Image postContentImage;
    [SerializeField] private TMP_Text postContentText;
    private ScriptableObject content;
    private Animator anim;
/*
    [SerializeField] private Renderer pbMaterial;
    private TextMeshProUGUI title;
    private TextMeshProUGUI additionalInfos;

    private TextMeshProUGUI contentText;

    private Transform personParent;

    private Transform contentParent;*/
    // Big image
    
    [SerializeField] private GameObject bigPictureParent;
    [SerializeField] private Renderer bigPicture;

    private List<string> newInfo = new List<string>();

    // Start is called before the first frame update
    void Start()
    {/*
        contentParent = transform.Find("Content");
        personParent = transform.Find("Person");
        // image = personParent.Find("PB").GetComponent<UnityEngine.UI.Image>();
        title = personParent.Find("Titel").GetComponent<TextMeshProUGUI>();
        additionalInfos = personParent.Find("AdditionalInfos/AdditionalInfosText").GetComponent<TextMeshProUGUI>();

        contentText = contentParent.Find("contentText").GetComponent<TextMeshProUGUI>();


        additionalInfos.text = "";*/
        anim = GetComponent<Animator>();

    }
    public void ShowInfo(bool b)
    {
        anim.SetBool("showInfo", b);
    }
    public void ShowInfo(bool b, ScriptableObject o)
    {
        if (o is IPinnable temp)
        {
            anim.SetBool("isLow", temp.isSmall);
        }
        if (o != content)
        {
            StartCoroutine(fastPullupCooldown());
            SetContent(o);
        }
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

    private IEnumerator fastPullupCooldown()
    {
        if (anim.GetBool("fast"))
        {
            yield break;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            anim.SetBool("fast", true);
            yield return new WaitForSeconds(3);
            anim.SetBool("fast", false);
        }
    }

    public void SetContent(ScriptableObject o)
    {
        // contentParent.gameObject.SetActive(!(o is Person || o is SocialMediaUser));
        // bigPicture.gameObject.SetActive(o is ArchiveData && ((ArchiveData)o).type == ArchiveType.Image);
        // personParent.gameObject.SetActive(o is Person || o is SocialMediaUser);
        // additionalInfos.text = "";
        // transform.Find("Image").gameObject.SetActive(true);
        // check if scribtable object is type person  
       personInfo.SetActive(false);
        userInfo.SetActive(false); 
        postContentInfo.SetActive(false);
        postContentUserImage.SetActive(false);
        bigPictureParent.SetActive(false);
        switch (o) 
        {
            case Person:
                Person p = (Person)o;
                backgroundClipboardImage.GetComponent<Image>().sprite = personInfoBG;
                personInfo.SetActive(true);
                personImage.sprite = p.image;
                personName.text = p.personName; 
                personContent.text = "";  
                foreach (string info in p.additionalInfos)
                {
                    personContent.text += "- " + info + "<br>";
                }
                break;
            case SocialMediaUser:
                SocialMediaUser smu = (SocialMediaUser)o;
                backgroundClipboardImage.GetComponent<Image>().sprite = userInfoBG;
                userInfo.SetActive(true);
                userImage.sprite = smu.image; 
                userName.text = smu.username;
                userContent.text = "";
                foreach (string info in smu.additionalInfos)
                {
                    userContent.text += "- " + info + "<br>";
                }
                break;
            case SocialMediaPost:
                SocialMediaPost smp = (SocialMediaPost)o;
                backgroundClipboardImage.GetComponent<Image>().sprite = postContentInfoBG;
                postContentInfo.SetActive(true);
                postContentUserImage.SetActive(true);
                postContentUserImage.transform.GetChild(0).GetComponent<Image>().sprite = smp.author.image;
                if (smp.image != null)  
                {
                    postContentImage.gameObject.SetActive(true);
                    postContentImage.sprite = smp.image;
                }
                else
                {
                    postContentImage.gameObject.SetActive(false);
                }
                postContentText.text = smp.content;
                break;
            case ArchiveData:
                ArchiveData a = (ArchiveData)o;
                postContentInfo.SetActive(true);
                backgroundClipboardImage.GetComponent<Image>().sprite = paperBG;
                if (a.type == ArchiveType.Image && a.image != null)
                {   
                    backgroundClipboardImage.SetActive(false);
                    postContentInfo.SetActive(false);
                    bigPictureParent.SetActive(true);
                    bigPicture.material.SetTexture("_Base", a.image.texture);
                }
                else if (a.image != null)
                {
                    //pbMaterial.material.SetTexture("_Base", a.image.texture);
                }
                else
                {
                    transform.Find("Image").gameObject.SetActive(false);
                }
                postContentText.text = a.content;
                break;
        }
    }


    // Update is called once per frame
    void Update()
    {

    }
}

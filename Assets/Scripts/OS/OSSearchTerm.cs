using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSSearchTerm : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI searchTermText;

    public SocialMediaUser user = null;

    private OSSocialMediaContent socialMediaContent;

    private void Start()
    {
        socialMediaContent = transform.GetComponentInParent<OSSocialMediaContent>();
    }

    public void SearchForTerm()
    {
        if (!user)
        {
            socialMediaContent.FilterHomefeed("#" + searchTermText.text);
        }
        else
        {
            socialMediaContent.ShowUserProfile(user);
        }
    }

}

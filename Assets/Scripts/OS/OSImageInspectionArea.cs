using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSImageInspectionArea : MonoBehaviour
{
    [TextArea] [SerializeField] private string inspectionText;
    [SerializeField] private SocialMediaUser exposedPasswordUser;

    private ComputerControls computerControls;
    private OSBigImageContent bigImageContent;

    // Start is called before the first frame update
    void Start()
    {
        computerControls = GetComponentInParent<ComputerControls>();
        bigImageContent = GetComponentInParent<OSBigImageContent>();
    }

    public void InspectArea()
    {
        computerControls.pointySystem.StartImageInspection(bigImageContent.relatedPost, inspectionText, exposedPasswordUser);
    }
}

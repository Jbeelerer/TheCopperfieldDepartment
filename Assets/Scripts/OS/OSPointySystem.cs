using SaveSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PointyTutorialStep
{
    public string targetObjectName;
    [TextArea] public string message;
    public bool pointAtPointy;
    public Vector2 spotlightSizeModifier;
    public string specificParentName;
    public int stepsToGetToSpecificParent;
}

public class OSPointySystem : MonoBehaviour, ISavable
{
    [SerializeField] private bool deactivatePointy = false;

    [SerializeField] private GameObject pointy;
    [SerializeField] private GameObject pointySpeechBubbleTop;
    [SerializeField] private GameObject pointySpeechBubbleBottom;
    [SerializeField] private GameObject screenBlockadePointy;
    [SerializeField] private GameObject pointyFinger;
    [SerializeField] private GameObject pinInspectionButton;
    [SerializeField] private AudioClip pointyPopupSound;

    public GameObject pointyButton;
    public GameObject spotlight;

    [Header("Tutorial Steps")]
    [SerializeField] private List<PointyTutorialStep> stepsDefault = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsDesktop = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsStartSettings = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsGovApp = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsPeopleList = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsPeopleListPinning = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsSocialMedia = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsSocialMediaPinning = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsSocialMediaProfiles = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsEvilIntro = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsEvilSocialMedia = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsInspectionTutorial = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsDmPasswordTutorial = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsTipsPageStartTutorial = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsTipsPageTutorial = new List<PointyTutorialStep>();

    [Header("Image Inspection")]
    [SerializeField] private List<PointyTutorialStep> stepsImageInspection = new List<PointyTutorialStep>();

    [HideInInspector] public bool evilIntroCompleted = false;

    private ComputerControls computerControls;
    private OSPopupManager popupManager;
    private GameObject nextTargetObject;
    private List<PointyTutorialStep> currentTutorial;
    private int currentStep;
    private List<string> completedTutorials = new List<string>();
    private Vector2 originalSpotlightSize;
    private Animator pointyAnim;

    private List<OSSocialMediaPost> pinnedInspectionPosts = new List<OSSocialMediaPost>();
    private SocialMediaPost inspectionOriginalPost = null;
    private void Awake()
    {
        pointyAnim = pointy.GetComponent<Animator>();
    }
    private void Start()
    {
        computerControls = GetComponentInParent<ComputerControls>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();

        spotlight.GetComponent<Image>().alphaHitTestMinimumThreshold = 1f;
        originalSpotlightSize = spotlight.GetComponent<RectTransform>().sizeDelta;
    }

    /*public void ToggleButtonNotif(string name)
    {
        if (!completedTutorials.Contains(name) && name != "Default")
        {
            pointyButton.GetComponent<Animator>().Play("buttonPointyNotif");
        }
        else
        {
            pointyButton.GetComponent<Animator>().Play("buttonPointyQuestion");
        }
    }*/

    public void StartTutorial(string name, bool toggledAutomatically = false)
    {
        ShowPointy(name, toggledAutomatically);
    }

    public IEnumerator StartTutorialDelayed(string name, float delay, bool toggledAutomatically = false)
    {
        yield return new WaitForSeconds(delay);
        computerControls.CloseAllWindows();
        StartTutorial(name, toggledAutomatically);
        yield break;
    }

    public void StartImageInspection(SocialMediaPost relatedPost, string text, SocialMediaUser exposedPasswordUser)
    {
        inspectionOriginalPost = relatedPost;

        // Add to list of users with found passwords, if a user password is exposed in this inspection area
        OSSocialMediaContent socialMediaContent = Object.FindObjectOfType<OSSocialMediaContent>();
        if (exposedPasswordUser && !socialMediaContent.GetUsersWithFoundPassword().Contains(exposedPasswordUser))
        {
            socialMediaContent.AddUserWithFoundPassword(exposedPasswordUser);
            computerControls.OnUserPasswordFound?.Invoke(exposedPasswordUser);
            computerControls.OpenWindow(OSAppType.WARNING, $"Password saved!<br><br>You can now log into <b>{exposedPasswordUser.username}'s</b> account!", DisplayPasswordFoundMsg, false);
        }

        ShowPointy("ImageInspection", false, text);
    }

    private void DisplayPasswordFoundMsg()
    {
        popupManager.DisplayPasswordFoundMessage();
    }

    private void ShowPointy(string name, bool toggledAutomatically, string inspectionText = null)
    {
        // Bool used to deactivate pointy in inspector
        if (deactivatePointy && name != "ImageInspection")
        {
            return;
        }

        if (name == "Default" && toggledAutomatically)
        {
            return;
        }

        // Add to completed list if not already there
        if (!CheckIfTutorialCompleted(name))
        {
            completedTutorials.Add(name);
        }
        else if (toggledAutomatically)
        {
            return;
        }

        // Play popup sound
        computerControls.audioManager.PlayAudio(pointyPopupSound);

        // Change button appearance
        pointyButton.GetComponent<Animator>().Play("buttonPointyClose");

        pointy.SetActive(true);

        switch (name)
        {
            case "Desktop":
                currentTutorial = stepsDesktop;
                break;
            case "StartSettings":
                if (toggledAutomatically)
                {
                    currentTutorial = stepsDesktop;
                }
                else
                {
                    currentTutorial = stepsStartSettings;
                }
                break;
            case "GovApp":
                currentTutorial = stepsGovApp;
                break;
            case "SocialMedia":
                currentTutorial = stepsSocialMedia;
                break;
            case "SocialMediaPinning":
                currentTutorial = stepsSocialMediaPinning;
                break;
            case "SocialMediaProfiles":
                currentTutorial = stepsSocialMediaProfiles;
                break;
            case "PeopleList":
                currentTutorial = stepsPeopleList;
                break;
            case "PeopleListPinning":
                currentTutorial = stepsPeopleListPinning;
                break;
            case "EvilIntro":
                currentTutorial = stepsEvilIntro;
                pointyAnim.Play("pointyEvilIdle");
                break;
            case "EvilSocialMedia":
                currentTutorial = stepsEvilSocialMedia;
                pointyAnim.Play("pointyEvilIdle");
                break;
            case "DmPassword":
                currentTutorial = stepsDmPasswordTutorial;
                break;
            case "TipsPageStart":
                currentTutorial = stepsTipsPageStartTutorial;
                break;
            case "TipsPage":
                currentTutorial = stepsTipsPageTutorial;
                break;
            case "InspectionTutorial":
                currentTutorial = stepsInspectionTutorial;
                pointyAnim.Play("pointyDetectiveIdle");
                break;
            case "ImageInspection":
                stepsImageInspection[1].message = inspectionText;
                pinInspectionButton.SetActive(true);
                currentTutorial = stepsImageInspection;
                pointyAnim.Play("pointyDetectiveIdle");
                break;
            case "Default":
            default:
                currentTutorial = stepsDefault;
                break;
        }

        // Reset size and position of affected window (if not inspection mode)
        if (computerControls.currentFocusedWindow && name != "ImageInspection")
        {
            computerControls.ResizeWindowSmall(computerControls.currentFocusedWindow);
            computerControls.currentFocusedWindow.rectTrans.position = computerControls.screen.position;
        }

        currentStep = 0;

        ProgressPointy();
    }

    public void ProgressPointy()
    {
        if (currentStep == currentTutorial.Count)
        {
            HidePointy();
            return;
        }

        pointyButton.GetComponent<Animator>().Play("buttonPointyClose");

        // Reset social media to home feed at start of its tutorial
        if (currentTutorial == stepsSocialMediaPinning && currentStep == 1 ||
            currentTutorial == stepsSocialMediaProfiles && currentStep == 1 ||
            currentTutorial == stepsEvilSocialMedia && currentStep == 4)
        {
            OSSocialMediaContent socialMediaContent = Object.FindObjectOfType<OSSocialMediaContent>();
            socialMediaContent.ResetHomeFeed();
            socialMediaContent.ShowHomeFeed();
            socialMediaContent.EnableFirstPostOptions();
        }

        screenBlockadePointy.SetActive(true);
        spotlight.SetActive(true);

        PointyTutorialStep step = currentTutorial[currentStep];
        nextTargetObject = GameObject.Find(step.targetObjectName);

        // Exceptions: Define more specific target object through a unique parent name if multiple with the same name exist at once
        // TODO: Find cleaner solution for such exceptions in the future
        if (!string.IsNullOrEmpty(currentTutorial[currentStep].specificParentName))
        {
            var objectsWithSameName = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == step.targetObjectName);
            foreach (var obj in objectsWithSameName)
            {
                Transform currentParent = obj.transform;
                for (int i = 0; i < currentTutorial[currentStep].stepsToGetToSpecificParent; i++)
                {
                    currentParent = currentParent.parent;
                }
                if (currentParent.name == currentTutorial[currentStep].specificParentName)
                {
                    nextTargetObject = obj;
                    break;
                }
            }
        }
        
        spotlight.GetComponent<RectTransform>().sizeDelta = originalSpotlightSize;
        // Resize spotlight if custom size is set
        if (step.spotlightSizeModifier != Vector2.zero)
        {
            spotlight.GetComponent<RectTransform>().sizeDelta = spotlight.GetComponent<RectTransform>().sizeDelta * step.spotlightSizeModifier;
        }

        if (!nextTargetObject)
        {
            Debug.LogError("Could not find pointy target object: " + step.targetObjectName + ". Does it not exist in the current window or is it deactivated?");
        }

        if (nextTargetObject == screenBlockadePointy)
        {
            spotlight.SetActive(false);
        }
        else
        {
            screenBlockadePointy.SetActive(false);
        }

        // Position pointy next to target object
        pointy.transform.position = nextTargetObject.transform.position + new Vector3(0.5f, 0, 0);

        spotlight.transform.position = nextTargetObject.transform.position;

        pointySpeechBubbleTop.SetActive(false);
        pointySpeechBubbleBottom.SetActive(false);
        if (pointy.GetComponent<RectTransform>().anchoredPosition.y <= 0)
        {
            pointySpeechBubbleTop.SetActive(true);
            pointySpeechBubbleTop.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = step.message;
        }
        else
        {
            pointySpeechBubbleBottom.SetActive(true);
            pointySpeechBubbleBottom.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = step.message;
        }

        if (step.pointAtPointy)
        {
            pointyFinger.gameObject.SetActive(true);
            //pointyButton.GetComponent<Button>().enabled = false;
        }
        else
        {
            pointyFinger.gameObject.SetActive(false);
            //pointyButton.GetComponent<Button>().enabled = true;
        }

        currentStep++;
    }

    public void HidePointy()
    {
        // Change button appearance
        pointyButton.GetComponent<Animator>().Play("buttonPointyQuestion");
        pointyAnim.Play("pinnyIdle");

        // Show social media notif if PeopleList tutorial complete on day 1
        if (currentTutorial == stepsPeopleList && GameManager.instance.GetDay() == 1 && !computerControls.CheckIfWindowIsOpen(OSAppType.SOCIAL))
        {
            computerControls.TriggerAppNotification(OSAppType.SOCIAL);
        }

        // Show delayed tips tutorial if day 2 people list and social media tutorials are completed
        if (CheckIfTutorialCompleted("SocialMediaPinning") && CheckIfTutorialCompleted("PeopleListPinning") && !CheckIfTutorialCompleted("TipsPageStart"))
        {
            StartCoroutine(StartTutorialDelayed("TipsPageStart", 4f, true));
        }

        nextTargetObject = null;
        currentTutorial = null;
        pointy.SetActive(false);
        spotlight.SetActive(false);
        screenBlockadePointy.SetActive(false);
        pointyFinger.gameObject.SetActive(false);
        pinInspectionButton.SetActive(false);
    }

    public GameObject GetNextTargetObject()
    {
        return nextTargetObject;
    }

    public void PinInspection()
    {
        StartCoroutine(PinInspectionCoroutine());
    }

    private IEnumerator PinInspectionCoroutine()
    {
        OSSocialMediaContent socialMediaContent = computerControls.GetComponentInChildren<OSSocialMediaContent>(true);
        OSSocialMediaPost inspectionPost = socialMediaContent.postList.Find(p => p.post.content == stepsImageInspection[1].message);

        if (pinnedInspectionPosts.Contains(inspectionPost))
        {
            socialMediaContent.UnpinPost(inspectionPost.post);
            pinnedInspectionPosts.Remove(inspectionPost);
            //yield break;
        }

        if (!inspectionPost)
        {
            SocialMediaPost newPost = ScriptableObject.CreateInstance<SocialMediaPost>();
            newPost.id = -1;
            newPost.author = inspectionOriginalPost.author;
            newPost.contentShort = stepsImageInspection[1].message;
            newPost.content = stepsImageInspection[1].message;
            newPost.image = null;
            newPost.hiddenInHomeFeed = true;

            socialMediaContent.InstanciatePost(newPost, isInspection: true);
            inspectionPost = socialMediaContent.postList[socialMediaContent.postList.Count - 1];
        }

        yield return null;

        socialMediaContent.PinPost(inspectionPost.post);
        pinnedInspectionPosts.Add(inspectionPost);
    }

    public bool GetIsPointyActive()
    {
        return pointy.activeSelf;
    }

    public bool CheckIfTutorialCompleted(string name)
    {
        return completedTutorials.Contains(name);
    }

    public void LoadData(SaveData data)
    {
        completedTutorials = data.completedTutorials;
    }

    public void SaveData(SaveData data)
    {
        data.completedTutorials = completedTutorials;
    }
}

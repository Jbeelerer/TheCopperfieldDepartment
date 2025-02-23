using SaveSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PointyTutorialStep
{
    public string targetObjectName;
    public string message;
    public bool pointAtPointy;
    public Vector2 spotlightSizeModifier;
}

public class OSPointySystem : MonoBehaviour
{
    [SerializeField] private bool deactivatePointy = false;

    [SerializeField] private GameObject pointy;
    [SerializeField] private GameObject pointySpeechBubbleTop;
    [SerializeField] private GameObject pointySpeechBubbleBottom;
    [SerializeField] private GameObject screenBlockadePointy;
    [SerializeField] private GameObject pointyFinger;
    [SerializeField] private AudioClip pointyPopupSound;

    public GameObject pointyButton;
    public GameObject spotlight;

    [SerializeField] private List<PointyTutorialStep> stepsDefault = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsDesktop = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsStartSettings = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsGovApp = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsPeopleList = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsSocialMedia = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsEvilIntro = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsEvilSocialMedia = new List<PointyTutorialStep>();

    [HideInInspector] public bool evilIntroCompleted = false;

    private ComputerControls computerControls;
    private GameObject nextTargetObject;
    private List<PointyTutorialStep> currentTutorial;
    private int currentStep;
    private List<string> completedTutorials = new List<string>();
    private Vector2 originalSpotlightSize;
    private Animator pointyAnim;

    private void Start()
    {
        computerControls = GetComponentInParent<ComputerControls>();
        pointyAnim = pointy.GetComponent<Animator>();

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
        // Bool used to deactivate pointy in inspector
        if (deactivatePointy)
        {
            return;
        }

        if (name == "Default" && toggledAutomatically)
        {
            return;
        }

        // Add to completed list if not already there
        if (!completedTutorials.Contains(name))
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
            case "PeopleList":
                currentTutorial = stepsPeopleList;
                break;
            case "EvilIntro":
                currentTutorial = stepsEvilIntro;
                pointyAnim.Play("pointyEvilIdle");
                break;
            case "EvilSocialMedia":
                currentTutorial = stepsEvilSocialMedia;
                pointyAnim.Play("pointyEvilIdle");
                break;
            case "Default":
            default:
                currentTutorial = stepsDefault;
                break;
        }

        // Reset size and position of affected window
        if (computerControls.currentFocusedWindow) {
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
        if (currentTutorial == stepsSocialMedia && currentStep == 1
            || currentTutorial == stepsEvilSocialMedia && currentStep == 4)
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

        nextTargetObject = null;
        currentTutorial = null;
        pointy.SetActive(false);
        spotlight.SetActive(false);
        screenBlockadePointy.SetActive(false);
        pointyFinger.gameObject.SetActive(false);
    }

    public GameObject GetNextTargetObject()
    {
        return nextTargetObject;
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

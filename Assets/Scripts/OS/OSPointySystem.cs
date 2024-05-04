using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class PointyTutorialStep
{
    public string targetObjectName;
    public string message;
    public bool pointAtPointy;
}

public class OSPointySystem : MonoBehaviour
{
    [SerializeField] private bool deactivatePointy = false;

    [SerializeField] private GameObject pointy;
    [SerializeField] private GameObject pointySpeechBubbleTop;
    [SerializeField] private GameObject pointySpeechBubbleBottom;
    [SerializeField] private GameObject screenBlockadePointy;
    [SerializeField] private GameObject pointyFinger;

    public GameObject pointyButton;
    public GameObject spotlight;

    [SerializeField] private List<PointyTutorialStep> stepsDefault = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsDesktop = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsGovApp = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsPeopleList = new List<PointyTutorialStep>();
    [SerializeField] private List<PointyTutorialStep> stepsSocialMedia = new List<PointyTutorialStep>();

    private GameObject nextTargetObject;
    private List<PointyTutorialStep> currentTutorial;
    private int currentStep;

    private void Start()
    {
        spotlight.GetComponent<Image>().alphaHitTestMinimumThreshold = 1f;
    }

    public void StartTutorial(string name)
    {
        if (deactivatePointy)
        {
            return;
        }

        switch (name)
        {
            case "Desktop":
                currentTutorial = stepsDesktop;
                break;
            case "GovApp":
                currentTutorial = stepsGovApp;
                break;
            case "SocialMedia":
                currentTutorial = stepsSocialMedia;
                break;
            /*case "PeopleList":
                currentTutorial = stepsPeopleList;
                break;*/
            default:
                currentTutorial = stepsDefault;
                break;
        }

        pointy.SetActive(true);
        screenBlockadePointy.SetActive(true);
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

        PointyTutorialStep step = currentTutorial[currentStep];
        nextTargetObject = GameObject.Find(step.targetObjectName);

        if (nextTargetObject == screenBlockadePointy)
        {
            screenBlockadePointy.SetActive(true);
            spotlight.SetActive(false);
        }
        else
        {
            spotlight.SetActive(true);
            screenBlockadePointy.SetActive(false);
        }

        pointy.transform.position = nextTargetObject.transform.position + new Vector3(1, 0, 0);

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
        }
        else
        {
            pointyFinger.gameObject.SetActive(false);
        }

        currentStep++;
    }

    public void HidePointy()
    {
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
}

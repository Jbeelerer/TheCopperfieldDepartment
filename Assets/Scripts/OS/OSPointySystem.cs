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
}

public class OSPointySystem : MonoBehaviour
{
    [SerializeField] private bool deactivatePointy = false;

    [SerializeField] private GameObject pointy;
    [SerializeField] private GameObject pointySpeechBubbleTop;
    [SerializeField] private GameObject pointySpeechBubbleBottom;
    [SerializeField] private GameObject screenBlockadePointy;

    public GameObject spotlight;

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
                return;
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
            nextTargetObject = null;
            currentTutorial = null;
            HidePointy();
            return;
        }

        PointyTutorialStep step = currentTutorial[currentStep];
        nextTargetObject = GameObject.Find(step.targetObjectName);
        pointy.transform.position = nextTargetObject.transform.position + new Vector3(1, 0, 0);

        spotlight.transform.position = nextTargetObject.transform.position;

        if (nextTargetObject == screenBlockadePointy)
        {
            screenBlockadePointy.SetActive(true);
            spotlight.SetActive(false);
        }

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

        currentStep++;
    }

    private void HidePointy()
    {
        pointy.SetActive(false);
        spotlight.SetActive(false);
    }

    public void HideBlockade()
    {
        screenBlockadePointy.SetActive(false);
        spotlight.SetActive(true);
    }

    public GameObject GetNextTargetObject()
    {
        return nextTargetObject;
    }
}

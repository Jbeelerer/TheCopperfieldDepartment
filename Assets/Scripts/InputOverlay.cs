using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputOverlay : MonoBehaviour
{

    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private Sprite handOpen;
    [SerializeField] private Sprite handClosed;
    [SerializeField] private Sprite handThread;
    [SerializeField] private Sprite trash;
    [SerializeField] private Sprite scissors;
    [SerializeField] private Sprite inspect;

    private Slider onHoldDisplay;

    private Image image;

    private bool isHolding = false;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        onHoldDisplay = transform.GetComponentInChildren<Slider>();
        onHoldDisplay.gameObject.SetActive(false);
        image.enabled = false;
        GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
    }
    public void SetIcon(string imageName)
    {
        image.enabled = true;
        // don't allow any icon changes while moving pins
        if (image.sprite == handClosed && imageName != "handOpen")
        {
            return;
        }
        switch (imageName)
        {
            case "default":
                image.sprite = defaultIcon;
                break;
            case "handOpen":
                image.sprite = handOpen;
                break;
            case "handClosed":
                image.sprite = handClosed;
                break;
            case "handThread":
                image.sprite = handThread;
                break;
            case "trash":
                image.sprite = trash;
                break;
            case "scissors":
                image.sprite = scissors;
                break;
            case "inspect":
                image.sprite = inspect;
                break;
            default:
                image.enabled = false;
                break;
        }
    }

    public void startHold(float duration)
    {
        if (isHolding)
        {
            stopHold();
        }
        onHoldDisplay.maxValue = duration;
        isHolding = true;
        onHoldDisplay.gameObject.SetActive(true);
        StartCoroutine(holdDown(duration));
    }

    public void stopHold()
    {
        isHolding = false;
        onHoldDisplay.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    private IEnumerator holdDown(float duration)
    {
        float time = 0;
        while (time < duration && isHolding)
        {
            onHoldDisplay.value = time;
            time += Time.deltaTime;
            yield return null;
        }
        stopHold();
    }


    // Update is called once per frame
    void Update()
    {

    }
}

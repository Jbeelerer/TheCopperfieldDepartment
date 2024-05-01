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
    [SerializeField] private Sprite exit;
    [SerializeField] private Sprite pen;
    [SerializeField] private Sprite draw;


    private Slider onHoldDisplay;

    private Image image;

    private bool isHolding = false;

    public bool GetIsHolding()
    {
        return isHolding;
    }
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        onHoldDisplay = transform.GetComponentInChildren<Slider>();
        onHoldDisplay.gameObject.SetActive(false);
        image.enabled = false;
        GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
    }
    public void SetIcon(string imageName, bool forceIcon = false)
    {
        image.enabled = true;
        // don't allow any icon changes while moving pins
        if (!forceIcon && image.sprite == handClosed && imageName != "handOpen")
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
            case "exit":
                image.sprite = exit;
                break;
            case "pen":
                image.sprite = pen;
                break;
            case "draw":
                image.sprite = draw;
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

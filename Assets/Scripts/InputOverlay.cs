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
    [SerializeField] private Sprite pin;

    private Slider onHoldDisplay;

    [SerializeField] private Image image;
    [SerializeField] private Image bg;

    private bool isHolding = false;

    private bool followCursor = false;

    private Vector3 defaultPosition;

    private string currentIcon;

    public bool GetIsHolding()
    {
        return isHolding;
    }
    // Start is called before the first frame update
    void Start()
    {
        // image = GetComponent<Image>();
        onHoldDisplay = transform.GetComponentInChildren<Slider>();
        onHoldDisplay.gameObject.SetActive(false);
        image.enabled = false;
        bg.enabled = false;
        GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
        defaultPosition = transform.position;
    }
    public void SetIfFollowCursor(bool b)
    {
        followCursor = b;
        if (!b)
        {
            transform.position = defaultPosition;
        }
    }
    public void SetIcon(string imageName, bool forceIcon = false)
    {
        currentIcon = imageName;
        image.enabled = true;
        bg.enabled = true;
        // don't allow any icon changes while moving pins only empty
        if (!forceIcon && image.sprite == handClosed && imageName != "handOpen" && imageName != "")
        {
            return;
        }
        switch (imageName)
        {
            case "defaultIcon":
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
            case "pin":
                image.sprite = pin;
                break;
            default:
                image.sprite = defaultIcon;
                image.enabled = false;
                bg.enabled = false;
                break;
        }
    }

    public void ChangeIconIfDifferent(string imageName)
    {
        if (currentIcon != imageName)
        {
            SetIcon(imageName);
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
    private void FixedUpdate()
    {
        if (followCursor)
        {
            transform.position = Input.mousePosition;
        }
    }
}

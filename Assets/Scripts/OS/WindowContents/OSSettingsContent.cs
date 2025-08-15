using SaveSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSSettingsContent : MonoBehaviour
{
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Transform cursorShowcaseSprites;

    private ComputerControls computerControls;

    private void OnEnable()
    {
        // Load current settings
        if (!computerControls)
            computerControls = transform.GetComponentInParent<ComputerControls>();

        sensitivitySlider.value = computerControls.GetMouseSensitivity();
        SetCursorShowcaseSprites();
    }

    public void ChangeSensitivity()
    {
        computerControls.SetMouseSensitivity(sensitivitySlider.value);
    }

    public void ChangeCursorSkin(bool cyclingForward)
    {
        computerControls.SwitchCursorSkin(cyclingForward);

        SetCursorShowcaseSprites();
    }

    private void SetCursorShowcaseSprites()
    {
        Sprite[] newCursorSprites = computerControls.GetCursorSprites();
        for (int i = 0; i < cursorShowcaseSprites.childCount; i++)
        {
            cursorShowcaseSprites.GetChild(i).GetComponent<Image>().sprite = newCursorSprites[i];
        }
    }

    public void ConfirmSettings()
    {
        // Save current settings  
        computerControls.SetMouseSensitivity(sensitivitySlider.value);
    }
}

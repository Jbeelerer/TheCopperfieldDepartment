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

    private ComputerControls computerControls;

    private void OnEnable()
    {
        // Load current settings
        if (!computerControls)
            computerControls = transform.GetComponentInParent<ComputerControls>();

        sensitivitySlider.value = computerControls.GetMouseSensitivity();
    }

    public void ChangeSensitivity()
    {
        computerControls.SetMouseSensitivity(sensitivitySlider.value);
    }

    public void ConfirmSettings()
    {
        // Save current settings  
        computerControls.SetMouseSensitivity(sensitivitySlider.value);
    }
}

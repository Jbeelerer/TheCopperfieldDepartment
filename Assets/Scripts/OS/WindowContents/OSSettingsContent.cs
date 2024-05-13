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

        sensitivitySlider.value = computerControls.mouseSensitivity;
    }

    public void ChangeSensitivity()
    {
        computerControls.mouseSensitivity = sensitivitySlider.value;
    }

    public void ConfirmSettings()
    {
        // Save current settings
        computerControls.mouseSensitivity = sensitivitySlider.value;
    }
}

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
    [SerializeField] private Image cursorImage1;
    [SerializeField] private Texture2D[] cursorSkinTextures;

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

    public void ChangeCursorSkin(bool forward)
    {
        if (forward)
        {
            cursorImage1.sprite = Sprite.Create(cursorSkinTextures[0], new Rect(0, 32, 32, 32), new Vector2(0.5f, 0.5f));
        }
    }

    public void ConfirmSettings()
    {
        // Save current settings  
        computerControls.SetMouseSensitivity(sensitivitySlider.value);
    }
}

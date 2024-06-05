using SaveSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSStartSettingsContent : MonoBehaviour
{
    [SerializeField] private Slider sensitivitySlider;
    private ComputerControls computerControls;
    private GameObject screenBlockadeBG;
    private GameObject screenBlockadeTaskBar;

    private void Start()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();

        screenBlockadeBG = GameObject.Find("ScreenBlockadeBG");
        screenBlockadeBG.GetComponent<Image>().enabled = true;
        screenBlockadeBG.transform.SetAsLastSibling();

        screenBlockadeTaskBar = GameObject.Find("ScreenBlockadeTaskBar");
        screenBlockadeTaskBar.GetComponent<Image>().enabled = true;

        transform.GetComponentInParent<OSWindow>().transform.SetAsLastSibling();

        computerControls.SetMouseSensitivityModifier(Screen.height / 30);
        computerControls.SetMouseSensitivity(sensitivitySlider.value);

        //sensitivitySlider.value = mouseSensitivityRaw;
    }

    public void ChangeSensitivity()
    {
        computerControls.SetMouseSensitivity(sensitivitySlider.value);
    }

    public void ConfirmSettings()
    {
        computerControls.SetMouseSensitivity(sensitivitySlider.value); ;
        computerControls.TogglePointy(true);

        screenBlockadeBG.GetComponent<Image>().enabled = false;
        screenBlockadeTaskBar.GetComponent<Image>().enabled = false;
        Destroy(GetComponentInParent<OSWindow>().associatedTab.gameObject);
        Destroy(GetComponentInParent<OSWindow>().gameObject);
    }
}

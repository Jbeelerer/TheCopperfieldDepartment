using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LastDayReport : MonoBehaviour
{
    private GameManager gm;
    private TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        text = GetComponentInChildren<TextMeshProUGUI>();
        gm.OnNewDay.AddListener(UpdateText);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void UpdateText()
    {
        switch (gm.GetInvestigationState())
        {
            case investigationStates.SuspectFound:
                // plus points 
                text.text = "You'r investigation lead to the capture of a target! Congratulation!";
                break;

            case investigationStates.SuspectNotFound:
                // minus points 
                text.text = "You didn't do your work properly. A colleague of yours was able to do it for you though...";
                break;

            case investigationStates.SuspectSaved:
                // no points 
                text.text = "We weren't able to find anybody yesterday.";
                break;

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitLight : MonoBehaviour
{
    private GameManager gm;
    [SerializeField] private Color exitLightColor;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        gm.InvestigationStateChanged.AddListener(UpdateLight);
        gm.OnNewDay.AddListener(UpdateLight);
    }

    void UpdateLight()
    {
        if (gm.GetAnswerCommited())
        {
            transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", exitLightColor * 3);
            transform.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            transform.GetComponent<Renderer>().material.SetColor("_EmissionColor", exitLightColor * 1);
            transform.transform.GetChild(0).gameObject.SetActive(false);
        }
        
    }
}

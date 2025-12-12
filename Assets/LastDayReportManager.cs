using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class LastDayReportManager : MonoBehaviour
{
    private Transform[] lastDayReports;
    private int currentReportAmount = 0;
    // Start is called before the first frame update
    void Start()
    {
        //get everychild
        lastDayReports = GetComponentsInChildren<Transform>();
        //disable them
        for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
    }
    public void AddLastDayReport(Texture2D canvasTexture)
    {
        currentReportAmount++;
        lastDayReports[currentReportAmount].gameObject.SetActive(true);
        Material mat = new Material(lastDayReports[currentReportAmount].GetComponent<Renderer>().material);
        mat.SetTexture("_SecondTexture", canvasTexture);
        lastDayReports[currentReportAmount].GetComponent<Renderer>().material = mat;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

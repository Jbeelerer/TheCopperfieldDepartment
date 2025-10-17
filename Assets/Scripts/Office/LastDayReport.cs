using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LastDayReport : MonoBehaviour
{
    private GameManager gm;
    [SerializeField] private TextMeshProUGUI suspectName;
    [SerializeField] private TextMeshProUGUI explenation;
    [SerializeField] private Image suspectImage;
    [SerializeField] private Image stamp;
    [SerializeField] private Sprite stampSuccess;
    [SerializeField] private Sprite stampFailed;
    [SerializeField] private GameObject newDayPrefab;


    // Start is called before the first frame update
    void Awake()
    {
        gm = GameManager.instance;

        suspectName.text = gm.GetCurrentlyAccused().personName;
        suspectImage.sprite = gm.GetCurrentlyAccused().image;
        explenation.text = gm.GetFeedBackExplanation();
        print(gm.GetFeedBackExplanation()); 
        print(gm.GetDay()-2); 
        if (gm.GetResultForDay(gm.GetDay()-2) == investigationStates.SuspectFound)
        {
            stamp.sprite = stampSuccess; 
        }
        else
        {
            stamp.sprite = stampFailed;
        }

    }
    public void Next()
    {
        GameObject g = Instantiate(newDayPrefab);
        GameManager.instance.reloadIfOver();
        Destroy(gameObject);
    }
}

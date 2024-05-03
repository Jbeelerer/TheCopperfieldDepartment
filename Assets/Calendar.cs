using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Calendar : MonoBehaviour
{
    [SerializeField] private GameObject dayPrefab;
    GameManager gm;

    [SerializeField] private Sprite suspectFound;
    [SerializeField] private Sprite SuspectNotFound;
    [SerializeField] private Sprite SuspectSaved;
    void Awake()
    {
        gm = GameManager.instance;
    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 1; i < 31; i++)
        {
            GameObject day = Instantiate(dayPrefab, transform);
            day.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (i).ToString();
            if (gm.GetFurthestDay() + 1 <= i)
            {
                day.transform.GetComponent<UnityEngine.UI.Button>().interactable = false;
            }
            if (gm.GetFurthestDay() > i)
            {
                day.transform.Find("FirstTryResult").GetComponent<UnityEngine.UI.Image>().sprite = SuspectNotFound;
                day.transform.Find("FirstTryResult").GetComponent<UnityEngine.UI.Image>().color = Color.red;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}

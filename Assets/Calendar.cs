using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using SaveSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Calendar : MonoBehaviour
{
    [SerializeField] private GameObject dayPrefab;
    GameManager gm;
    SaveManager sm;

    [SerializeField] private Sprite suspectFound;
    [SerializeField] private Sprite SuspectNotFound;
    [SerializeField] private Sprite SuspectSaved;


    void Awake()
    {
        gm = GameManager.instance;
        sm = SaveManager.instance;
        // sm.OnLoaded.AddListener(LoadCalendar);
    }
    // Start is called before the first frame update
    void Start()
    {
        LoadCalendar();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LoadCalendar()
    {
        print(gm.GetFurthestDay());
        for (int i = 1; i < 31; i++)
        {
            GameObject day = Instantiate(dayPrefab, transform);
            day.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (i).ToString();
            if (gm.GetFurthestDay() < i)
            {
                day.transform.GetComponent<UnityEngine.UI.Button>().interactable = false;
            }
            if (gm.GetFurthestDay() > i)
            {
                day.transform.Find("FirstTryResult").GetComponent<UnityEngine.UI.Image>().sprite = gm.GetResultForDay(i - 1) == investigationStates.SuspectFound ? suspectFound : gm.GetResultForDay(i - 1) == investigationStates.SuspectNotFound ? SuspectNotFound : SuspectSaved;
                day.transform.Find("FirstTryResult").GetComponent<UnityEngine.UI.Image>().color = gm.GetResultForDay(i - 1) == investigationStates.SuspectNotFound ? Color.red : Color.green;
            }
        }

    }

    public void ResetSave()
    {
        print("Resetting Save");
        sm.DeleteSave();
        sm.LoadGame();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        LoadCalendar();
    }

}

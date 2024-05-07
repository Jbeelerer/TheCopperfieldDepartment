using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using SaveSystem;
using TMPro;
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
        //transform.parent.GetComponent<Canvas>().worldCamera = Camera.main;
        // sm.OnLoaded.AddListener(LoadCalendar); 
    }
    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        sm = SaveManager.instance;
        gm.OnNewDay.AddListener(LoadCalendar);
        //LoadCalendar();
        StartCoroutine(LoadCalendarDelayed());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator LoadCalendarDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        LoadCalendar();
    }
    private void LoadCalendar()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 1; i < 31; i++)
        {
            GameObject day = Instantiate(dayPrefab, transform);
            day.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = (i).ToString();
            if (gm.GetFurthestDay() < i)
            {
                day.transform.GetComponent<UnityEngine.UI.Button>().interactable = false;
            }
            if (gm.GetDay() == i)
            {
                day.transform.GetComponentInChildren<TextMeshProUGUI>().fontWeight = TMPro.FontWeight.Bold;
            }
            else if (gm.GetDay() > i)
            {
                day.transform.Find("CalendarState").GetComponent<UnityEngine.UI.Image>().sprite = SuspectNotFound;
                day.transform.Find("CalendarState").GetComponent<UnityEngine.UI.Image>().color = new Color(1f, 0f, 0f, 0.5f);
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
        LoadCalendar();
    }

}

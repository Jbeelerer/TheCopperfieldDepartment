using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EmployeeRanking : MonoBehaviour
{
    private GameManager gm;
    private TextMeshProUGUI list;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        list = GetComponent<TextMeshProUGUI>();
        gm.OnNewDay.AddListener(UpdateList);
        UpdateList();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void UpdateList()
    {
        string content = "";
        gm.SortCompetingEmployees();
        foreach (CompetingEmployee e in gm.GetCompetingEmployees())
        {
            content += e.GetListString(gm.GetDay()) + "<br>";
        }
        list.text = content;
    }
}

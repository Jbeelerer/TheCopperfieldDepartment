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
        gm.OnNewDay.AddListener(UpdateList);
        list = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void UpdateList()
    {

        print("UpdateList");
        string content = "";
        foreach (CompetingEmployee e in gm.GetCompetingEmployees())
        {
            content += e.GetListString() + "<br>";
        }
        list.text = content;
    }
}

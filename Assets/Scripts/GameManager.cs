using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    /*
        private float time = 0;
        private float dayLength = 50;
        private int interval = 10;
        */

    private int day = 0;
    private int daySegment = 0;
    private int totalDaySegments = 0;

    private List<CompetingEmployee> competingEmployees = new List<CompetingEmployee>();
    private bool suspectFound = false;
    private bool suspectSaved = false;
    public UnityEvent OnNewDay;
    public UnityEvent OnNewSegment;

    public List<CompetingEmployee> GetCompetingEmployees()
    {
        return competingEmployees;
    }

    // Start is called before the first frame update
    void Awake()
    {
        // singleton
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }

        // initiate competing employees
        for (int i = 0; i < 10; i++)
        {
            competingEmployees.Add(new CompetingEmployee());
            print(competingEmployees[i].GetEmployeeName());
        }
    }
    void Start()
    {
        setNewDay();
    }

    public void setNewDay()
    {
        print("setNewDay");
        foreach (CompetingEmployee e in competingEmployees)
        {
            e.addNewPointsRandomly();
        }
        competingEmployees.Sort((x, y) =>
    y.GetPoints().CompareTo(x.GetPoints()));
        day++;
        daySegment = 0;
        OnNewDay?.Invoke();
    }
    private void initiateNewDay(int segments)
    {
        totalDaySegments = segments;
    }

    private void nextSegment()
    {
        daySegment++;
        OnNewSegment?.Invoke();
        if (daySegment >= totalDaySegments)
        {
            setNewDay();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    /*
    private void FixedUpdate()
    {
        // implement day 
        time += Time.deltaTime;
        // print(time % dayLength / intervalAmount == 0); 
        print((int)(time % interval));
        if (time % interval == 0)
        {
            print(time);
        }
        if (dayLength <= time)
        {
            day++;
            print("EndDay");
        }
    }*/
}

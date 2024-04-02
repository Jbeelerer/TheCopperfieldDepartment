using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum investigationStates
{
    SuspectNotFound,
    SuspectFound,
    SuspectSaved
}
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
    private CompetingEmployee playerOnEmployeeList;

    // what happens if the person is found and the post deleted?
    private investigationStates investigationState = investigationStates.SuspectNotFound;
    public UnityEvent OnNewDay;
    public UnityEvent OnNewSegment;
    private Case currentCase;

    public List<CompetingEmployee> GetCompetingEmployees()
    {
        return competingEmployees;
    }

    public investigationStates GetInvestigationState()
    {
        return investigationState;
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
        }
        playerOnEmployeeList = new CompetingEmployee("Player", 100, 0);
        competingEmployees.Add(playerOnEmployeeList);
    }
    void Start()
    {
        setNewDay();
    }

    public void setNewDay()
    {
        playerOnEmployeeList.addNewPoints(investigationState == investigationStates.SuspectFound ? 100 : -100);
        foreach (CompetingEmployee e in competingEmployees)
        {
            if (e != playerOnEmployeeList)
                e.addNewPointsRandomly();
        }
        competingEmployees.Sort((x, y) =>
    y.GetPoints().CompareTo(x.GetPoints()));
        day++;
        currentCase = Resources.Load<Case>("Case" + Random.Range(1, 2));
        daySegment = 0;
        OnNewDay?.Invoke();
    }
    public void checkSuspect(Person p)
    {
        if (p == currentCase.guiltyPerson)
        {
            investigationState = investigationStates.SuspectFound;
        }
    }
    public void checkDeletedPost(SocialMediaPost p)
    {
        if (p == currentCase.incriminatingPost && investigationState == investigationStates.SuspectNotFound)
        {
            investigationState = investigationStates.SuspectSaved;
        }
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

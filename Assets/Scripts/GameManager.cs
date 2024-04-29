using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

    private int day = 1;
    private int daySegment = 0;
    private int totalDaySegments = 0;

    private List<CompetingEmployee> competingEmployees = new List<CompetingEmployee>();
    private CompetingEmployee playerOnEmployeeList;

    // what happens if the person is found and the post deleted?
    private investigationStates investigationState = investigationStates.SuspectFound; //investigationStates.SuspectNotFound;
    public UnityEvent OnNewDay;
    public UnityEvent OnNewSegment;
    private Case currentCase;

    private Connections[] connections;
    [SerializeField] private SocialMediaPost[] posts;
    private SocialMediaUser[] users;
    private Person[] people;

    private Vector3 startPosition;
    private Quaternion startRotation;

    [SerializeField] private ScriptableObject customCase;

    [SerializeField] private GameObject newDayPrefab;

    public void SetStartTransform(Transform t)
    {
        startPosition = t.position;
        startRotation = t.rotation;
    }
    public Vector3 GetStartPosition()
    {
        return startPosition;
    }
    public Quaternion GetStartRotation()
    {
        return startRotation;
    }

    public Person[] GetPeople()
    {
        return people;
    }

    public SocialMediaUser[] GetUsers()
    {
        return users;
    }

    public SocialMediaPost[] GetPosts()
    {
        return posts;
    }

    public List<CompetingEmployee> GetCompetingEmployees()
    {
        return competingEmployees;
    }

    public investigationStates GetInvestigationState()
    {
        investigationStates result = investigationState;
        investigationState = investigationStates.SuspectNotFound;
        return result;
    }

    public Case GetCurrentCase()
    {
        return currentCase;
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
        setNewDay(true);
    }

    public void setNewDay(bool firstDay = false)
    {
        Instantiate(newDayPrefab);
        if (!firstDay)
        {
            playerOnEmployeeList.addNewPoints(investigationState == investigationStates.SuspectFound ? 100 : investigationState == investigationStates.SuspectSaved ? 0 : -100);
            foreach (CompetingEmployee e in competingEmployees)
            {
                if (e != playerOnEmployeeList)
                    e.addNewPointsRandomly();
            }
            day++;
        }
        competingEmployees.Sort((x, y) =>
    y.GetPoints().CompareTo(x.GetPoints()));
        if (Resources.LoadAll<Case>("Case" + day).Count() == 0)
        {
            // TODO: implement endgame   
            print("Game Over");
        }
        else
        {
            if (customCase == null)
                currentCase = Resources.LoadAll<Case>("Case" + day)[0];// Random.Range(1, 2))[0];//); 
            else
                currentCase = (Case)customCase;
        }
        // load all connections
        connections = Resources.LoadAll<Connections>("Case" + currentCase.id + "/Connections");
        posts = Resources.LoadAll<SocialMediaPost>("Case" + currentCase.id + "/Posts");

        //using lists to add new values dynamicly, afterwards convert to array, because it won't change and will be more performant
        List<Person> tempPeople = new List<Person>();
        List<SocialMediaUser> tempUsers = new List<SocialMediaUser>();
        foreach (SocialMediaPost p in posts)
        {
            if (!tempUsers.Contains(p.author))
            {
                tempUsers.Add(p.author);
                tempPeople.Add(p.author.realPerson);
            }
        }
        people = tempPeople.ToArray();
        users = tempUsers.ToArray();
        daySegment = 0;
        OnNewDay?.Invoke();
    }

    public string checkForConnectionText(ScriptableObject from, ScriptableObject to)
    {
        foreach (Connections c in connections)
        {
            if ((c.from.Contains(from) && c.to.Contains(to)) || (c.from.Contains(to) && c.to.Contains(from)))
            {
                return c.text;
            }
        }
        return "";
    }
    public void checkSuspect(Person p)
    {
        if (p == currentCase.guiltyPerson)
        {
            investigationState = investigationStates.SuspectFound;
        }
        else
        {
            investigationState = investigationStates.SuspectNotFound;
        }
    }
    public void checkDeletedPost(SocialMediaPost p)
    {
        if (p == currentCase.incriminatingPost)
        {
            investigationState = investigationStates.SuspectSaved;
        }
        else
        {
            investigationState = investigationStates.SuspectNotFound;
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

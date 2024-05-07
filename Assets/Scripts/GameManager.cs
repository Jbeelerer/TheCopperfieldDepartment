using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using SaveSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum investigationStates
{
    SuspectNotFound,
    SuspectFound,
    SuspectSaved
}
public enum GameState
{
    Playing,
    Frozen,
    Paused,

}
public class GameManager : MonoBehaviour, ISavable
{
    public static GameManager instance;
    private GameState gameState = GameState.Playing;
    /*
        private float time = 0;
        private float dayLength = 50;
        private int interval = 10;
        */

    [SerializeField] private int day = 1;
    private int furthestDay = 1;
    private int daySegment = 0;
    private int totalDaySegments = 0;

    private List<CompetingEmployee> competingEmployees = new List<CompetingEmployee>();
    private CompetingEmployee playerOnEmployeeList;

    // what happens if the person is found and the post deleted?
    public investigationStates investigationState = investigationStates.SuspectNotFound; //investigationStates.SuspectNotFound;
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

    private bool answerCommited = false;
    public Narration narration;


    private int saveFile;

    private SaveManager saveManager;


    private List<investigationStates> firstTryResults = new List<investigationStates>();
    private List<investigationStates> results = new List<investigationStates>();
    /*
        public int GetTotalPoints()
        {
            int totalPoints = 0;
            foreach (int points in pointsPerDay)
            {
                totalPoints += points;
            }
            return totalPoints;
        }*/
    public int GetDay()
    {
        return day;
    }
    public void SetGameState(GameState state)
    {
        gameState = state;
    }
    public bool isFrozen()
    {
        return gameState == GameState.Frozen;
    }
    public GameState GetGameState()
    {
        return gameState;
    }
    public investigationStates GetResultForDay(int i)
    {
        return results[i];
    }
    public investigationStates GetFirstTryResultsForDay(int i)
    {
        return firstTryResults[i];
    }
    public void SetSaveFile(int saveFile)
    {
        instance.saveFile = saveFile;
    }
    public int GetSaveFile()
    {
        return instance.saveFile;
    }

    public int GetFurthestDay()
    {
        return furthestDay;
    }

    public void SaveData(SaveData data)
    {
        if (Resources.LoadAll<Case>("Case" + furthestDay).Count() == 0)
        {
            //  return;
        }
        data.currentDay = furthestDay;
        data.result = results;
        data.firstTryResult = firstTryResults;
        List<SaveableEmployee> tempSE = new List<SaveableEmployee>();
        foreach (CompetingEmployee e in competingEmployees)
        {
            SaveableEmployee se = new SaveableEmployee(e.GetEmployeeName(), e.GetPoints(), e.GetSkill(), e.GetPointsPerDay());
            se.isPlayer = e == playerOnEmployeeList;
            tempSE.Add(se);
        }
        data.competingEmployees = tempSE;
    }
    public void LoadData(SaveData data)
    {
        print("load furthest day: " + data.currentDay);
        furthestDay = data.currentDay;
        results = data.result;
        firstTryResults = data.firstTryResult;
        competingEmployees.Clear();
        foreach (SaveableEmployee se in data.competingEmployees)
        {
            CompetingEmployee e = new CompetingEmployee(se.name, se.basePoints, se.skill);
            e.SetPointsPerDay(se.pointsPerDay);
            competingEmployees.Add(e);
            if (se.isPlayer)
            {
                playerOnEmployeeList = e;
            }
        }
        LoadNewDay(furthestDay);
    }
    public bool GetAnswerCommited()
    {
        return answerCommited;
    }
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


    }
    void Start()
    {
        saveManager = SaveManager.instance;

        if (Utility.CheckSaveFileExists(instance.saveFile.ToString()))
        {
            SaveManager.instance.SetupSaveFile(instance.saveFile.ToString());
            if (Utility.CheckSaveFileExists(instance.saveFile.ToString()))
            {
                print("loading.........");
                SaveManager.instance.LoadGame();
            }
            //Todo: can probably be removed... (Will do later <3)
            else
            {
                StartCoroutine(DelayFirstDay());
            }
        }
        else
        {
            StartCoroutine(DelayFirstDay());
        }

        // initiate competing employees
        for (int i = 0; i < 10; i++)
        {
            competingEmployees.Add(new CompetingEmployee());
        }
        playerOnEmployeeList = new CompetingEmployee("Player", 50, 0);
        competingEmployees.Add(playerOnEmployeeList);
    }

    public void SetNarration(Narration n)
    {
        narration = n;
    }


    public void LoadNewDay(int day)
    {
        this.day = day;
        if (Resources.LoadAll<Case>("Case" + day).Count() == 0)
        {
            // TODO: implement endgame   
            print("Game Over");
            //  return; 
            // todo: Only temp solution...
            day = 1;
        }
        if (customCase == null)
            currentCase = Resources.LoadAll<Case>("Case" + day)[0];// Random.Range(1, 2))[0];//); 
        else
            currentCase = (Case)customCase;

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
        SortCompetingEmployees();
        OnNewDay?.Invoke();
    }

    public void SortCompetingEmployees()
    {
        competingEmployees.Sort((x, y) =>
    y.GetTotalPoints(day).CompareTo(x.GetTotalPoints(day)));
    }

    public void PlayTimeTravelSequence()
    {
        GameObject g = Instantiate(newDayPrefab);
        g.GetComponent<Animator>().Play("TimeTravel");

    }

    public void setNewDay(bool firstDay = false)
    {
        int pointsThisDay = investigationState == investigationStates.SuspectFound ? 100 : investigationState == investigationStates.SuspectSaved ? 0 : -100;
        answerCommited = false;
        if (!firstDay)
        {
            Instantiate(newDayPrefab);
            playerOnEmployeeList.addNewPoints(pointsThisDay, day - 1);
            foreach (CompetingEmployee e in competingEmployees)
            {
                if (e.GetEmployeeName() != playerOnEmployeeList.GetEmployeeName())
                    e.addNewPointsRandomly(day - 1);
            }
            day++;
            if (day > furthestDay)
            {
                furthestDay = day;
            }

            if (results.Count <= furthestDay && furthestDay == day)
            {
                results.Add(investigationState);
                firstTryResults.Add(investigationState);
                // pointsPerDay.Add(pointsThisDay);
            }
            else
            {
                results[day - 2] = investigationState;
                // pointsPerDay[day - 2] = pointsThisDay;
            }
            SaveManager.instance.SaveGame();
        }
        LoadNewDay(day);
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
        answerCommited = true;
        narration.Say("suspectFound");
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
        answerCommited = true;
        narration.Say("deletePost");
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


    private IEnumerator DelayFirstDay()
    {
        LoadNewDay(day);
        yield return null;
    }
}

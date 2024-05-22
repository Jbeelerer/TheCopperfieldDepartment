using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SaveSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
    OnPC,
    OnCalendar,

}
public class GameManager : MonoBehaviour, ISavable
{
    public static GameManager instance;
    private GameState gameState = GameState.Playing;
    [SerializeField] private int day = 1;
    private int furthestDay = 1;
    private int daySegment = 0;
    private int totalDaySegments = 0;
    private List<CompetingEmployee> competingEmployees = new List<CompetingEmployee>();
    private CompetingEmployee playerOnEmployeeList;
    // what happens if the person is found and the post deleted?
    public investigationStates investigationState = investigationStates.SuspectNotFound; //investigationStates.SuspectNotFound;
    public UnityEvent OnNewDay;
    public UnityEvent StateChanged;
    public UnityEvent InvestigationStateChanged;
    public UnityEvent OnNewSegment;
    private Case currentCase;
    private Mail[] mails;
    private Connections[] connections;
    [SerializeField] private SocialMediaPost[] posts;
    private SocialMediaUser[] users;
    private Person[] people;

    private Vector3 startPosition;
    private Quaternion startRotation;
    [SerializeField] private GameObject newDayPrefab;

    private bool answerCommited = false;
    public Narration narration;
    private int saveFile;

    private SaveManager saveManager;

    private AudioManager am;
    [SerializeField] private AudioClip door;

    private bool devMode = true;


    private List<investigationStates> firstTryResults = new List<investigationStates>();
    private List<investigationStates> results = new List<investigationStates>();
    private GameObject computerCam;
    private GameObject calendarCam;

    private GameObject mainCam;

    private Person currentlyAccused;

    public bool GetIfDevMode()
    {
        return devMode;
    }
    public int GetDay()
    {
        return day;
    }
    public void SetGameState(GameState state)
    {
        reload();
        computerCam.SetActive(state == GameState.OnPC);
        if (calendarCam)
            calendarCam.SetActive(state == GameState.OnCalendar);

        mainCam.SetActive(state == GameState.Playing || state == GameState.Paused || state == GameState.Frozen);

        if (calendarCam)
            calendarCam.transform.parent.parent.GetComponent<Collider>().enabled = state != GameState.OnCalendar;

        Cursor.visible = state == GameState.OnCalendar;
        Cursor.lockState = state == GameState.OnPC || state == GameState.OnCalendar ? CursorLockMode.Confined : CursorLockMode.Locked;

        // handle startPos of mainCam
        if (state == GameState.Playing && gameState == GameState.OnPC)
        {
            GameObject.Find("Player").GetComponent<FPSController>().ResetCameraRotation(Quaternion.Euler(0, -70, 0), true);
        }
        gameState = state;
        StateChanged?.Invoke();
    }

    public bool isFrozen()
    {
        return gameState == GameState.OnPC || gameState == GameState.OnCalendar || gameState == GameState.Frozen;
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
        reload();

        // initiate competing employees
        for (int i = 0; i < 10; i++)
        {
            competingEmployees.Add(new CompetingEmployee());
        }
        playerOnEmployeeList = new CompetingEmployee("Player", 50, 0);
        competingEmployees.Add(playerOnEmployeeList);

    }
    public void reload()
    {
        if (mainCam == null)
            mainCam = GameObject.Find("Virtual Camera");
        if (computerCam == null)
            computerCam = GameObject.Find("ComputerCam");
        if (calendarCam == null)
            calendarCam = GameObject.Find("CalendarCam");
    }
    void Start()
    {
        saveManager = SaveManager.instance;
        am = AudioManager.instance;

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
                //LoadNewDay(day);
            }
        }
        else
        {
            StartCoroutine(DelayFirstDay());
            //LoadNewDay(day); 
        }
    }

    public void SetNarration(Narration n)
    {
        narration = n;
    }

    public Mail[] GetMails()
    {
        return mails;
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
        currentCase = Resources.LoadAll<Case>("Case" + day)[0];

        // load all connections
        connections = Resources.LoadAll<Connections>("Case" + currentCase.id + "/Connections");
        mails = Resources.LoadAll<Mail>("Case" + currentCase.id + "/Mails");
        posts = Resources.LoadAll<SocialMediaPost>("Case" + currentCase.id + "/Posts");
        //using lists to add new values dynamicly, afterwards convert to array, because it won't change and will be more performant
        List<Person> tempPeople = new List<Person>();
        List<SocialMediaUser> tempUsers = new List<SocialMediaUser>();
        foreach (Person p in currentCase.people)
        {
            tempPeople.Add(p);
        }
        foreach (SocialMediaPost p in posts)
        {
            if (!tempUsers.Contains(p.author))
            {
                tempUsers.Add(p.author);
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
    public void NextDaySequence()
    {
        GameObject g = Instantiate(newDayPrefab);
    }

    public void setNewDay(bool firstDay = false)
    {
        int pointsThisDay = investigationState == investigationStates.SuspectFound ? 100 : investigationState == investigationStates.SuspectSaved ? 0 : -100;
        answerCommited = false;
        if (!firstDay)
        {
            playerOnEmployeeList.addNewPoints(pointsThisDay, day - 1);
            foreach (CompetingEmployee e in competingEmployees)
            {
                if (e.GetEmployeeName() != playerOnEmployeeList.GetEmployeeName())
                    e.addNewPointsRandomly(day - 1);
            }
            day++;
            if (day == 2)
            {
                narration.PlaySequence(investigationState == investigationStates.SuspectFound ? "firstDayFeedbackPositive" : "firstDayFeedbackNegative");
            }
            else
            {
                NextDaySequence();
            }
            if (day > furthestDay)
            {
                furthestDay = day;
            }

            if (results.Count <= furthestDay && furthestDay == day)
            {
                results.Add(investigationState);
                firstTryResults.Add(investigationState);
            }
            else
            {
                results[day - 2] = investigationState;
            }
            // don't save in dev mode, if you want to, just add comment syntax to the if statement
            // if (!devMode) 
            SaveManager.instance.SaveGame();
        }
        LoadNewDay(day);
    }

    public Connections checkForConnectionText(ScriptableObject from, ScriptableObject to)
    {
        foreach (Connections c in connections)
        {
            if ((c.from.Contains(from) && c.to.Contains(to)) || (c.from.Contains(to) && c.to.Contains(from)))
            {
                return c;
            }
        }
        return null;
    }
    public bool checkIfPersonAccused(Person p)
    {
        return p == currentlyAccused;
    }
    public void checkSuspicionRemoved(Person p)
    {
        currentlyAccused = null;
        if (p == currentCase.guiltyPerson && investigationState == investigationStates.SuspectFound)
        {
            investigationState = investigationStates.SuspectNotFound;
            answerCommited = false;
        }
        InvestigationStateChanged?.Invoke();
    }
    public void checkSuspect(Person p)
    {
        currentlyAccused = p;
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
        InvestigationStateChanged?.Invoke();
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
        InvestigationStateChanged?.Invoke();
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
        if (Input.GetKeyDown(KeyCode.O) && devMode)
        {
            furthestDay = 1;
            LoadNewDay(1);
            SaveManager.instance.SaveGame();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (Input.GetKeyDown(KeyCode.P) && Input.GetKeyDown(KeyCode.O) && devMode)
        {
            Application.Quit();
        }
    }


    private IEnumerator DelayFirstDay()
    {
        LoadNewDay(day);
        yield return null;
    }
}

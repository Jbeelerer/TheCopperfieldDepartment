using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using SaveSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System;



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
    Inspecting,
    DayOver,
    InArchive

}
public class GameManager : MonoBehaviour, ISavable
{
    public static GameManager instance;
    private GameState gameState = GameState.Playing;
    private GameState prevGameState = GameState.Playing;
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
    public UnityEvent BeforeStateChanged;
    public UnityEvent InvestigationStateChanged;
    public UnityEvent OnNewSegment;
    private Case currentCase;
    [SerializeField] private int devCase;
    [SerializeField] private Mail[] mails;
    private Connections[] connections;
    [SerializeField] private SocialMediaPost[] posts;
    private SocialMediaUser[] users;
    private Person[] people;
    private DMConversation[] conversations;
    private ArchiveData[] archiveData;

    private Material[] seenReports;

    [SerializeField] private string[] dayOrder;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Quaternion startCameraRotation;
    [SerializeField] private GameObject newDayPrefab;
    [SerializeField] private GameObject feedbackReportPrefab;
    [SerializeField] private GameObject dayIntro;
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
    private GameObject inspectionCam;

    private GameObject mainCam;
    private investigationStates currentInvestigationState = investigationStates.SuspectNotFound;

    private Person currentlyAccused;

    private string feedBackExplanation = "";

    private bool _pinboardBlocked = false;

    public bool calendarLoad = false;
    private GameObject instantiatedDayIntro;

    private bool instantiateLoadedDay = false;

    private Pinboard pinboard;
    [SerializeField] private GameObject lastSuspectForm;

    public bool GetInstantiateLoadedDay()
    {
        return instantiateLoadedDay;
    }
    public bool PinboardBlocked
    {
        get { return _pinboardBlocked; }
        set
        {
            GameObject.Find("Pinboard").GetComponent<Pinboard>().pinboardInteractions(value);
            _pinboardBlocked = value;
        }
    }

    // wait for the new scene and the pinboard to be ready 
    private IEnumerator dalayedPinboardBlocked(bool t)
    {
        yield return new WaitForSeconds(0.5f);
        PinboardBlocked = t;
    }

    public bool GetIfDevMode()
    {
        return devMode;
    }
    public int GetDay()
    {
        return day;
    }
    public void ResetGame()
    {
        day = 1;
        furthestDay = 1;
        results.Clear();
        firstTryResults.Clear();
        competingEmployees.Clear();
        for (int i = 0; i < 10; i++)
        {
            competingEmployees.Add(new CompetingEmployee());
        }
        playerOnEmployeeList = new CompetingEmployee("Player", 50, 0);
        competingEmployees.Add(playerOnEmployeeList);
    }
    public void LookAt(Transform t)
    {
        inspectionCam.GetComponent<CinemachineVirtualCamera>().LookAt = t;
    }

    public void InspectObject(Transform o, Vector3 lookingDirection, GameState state = GameState.Inspecting)
    {
        if (inspectionCam == null)
        {
            reload();
        }
        CinemachineVirtualCamera vcam = inspectionCam.GetComponent<CinemachineVirtualCamera>();
        if (vcam.LookAt == o)
        {
            vcam.LookAt = null;
            SetGameState(GameState.Playing);
        }
        else
        {
            inspectionCam.transform.position = o.position + lookingDirection;
            vcam.LookAt = o;
            SetGameState(state);
        }
    }
    public void SetGameState(GameState state)
    {
        // PinboardBlocked = day == 1;
        prevGameState = gameState;
        BeforeStateChanged?.Invoke();

        if (state == GameState.Playing && inspectionCam != null)
        {
            inspectionCam.GetComponent<CinemachineVirtualCamera>().LookAt = null;
        }
        reload();
        computerCam.SetActive(state == GameState.OnPC);
        if (inspectionCam)
            inspectionCam.SetActive(state == GameState.Inspecting ||state == GameState.DayOver|| state == GameState.InArchive);

        mainCam.SetActive(state == GameState.Playing ||state == GameState.DayOver || state == GameState.Paused || state == GameState.Frozen);

        if (inspectionCam) 
            inspectionCam.transform.parent.parent.GetComponent<Collider>().enabled = (state != GameState.Inspecting && state != GameState.DayOver && state != GameState.InArchive);

        Cursor.visible = (state == GameState.Inspecting  ||state == GameState.DayOver|| state == GameState.InArchive);
        Cursor.lockState = (state == GameState.Inspecting  ||state == GameState.DayOver|| state == GameState.InArchive) ? CursorLockMode.Confined : CursorLockMode.Locked;
    
        // handle startPos of mainCam
        if (state == GameState.Playing && gameState == GameState.OnPC)
        {
            GameObject.Find("Player").GetComponent<FPSController>().ResetCameraRotation(Quaternion.Euler(0, -70, 0), true);
        }
        gameState = state;
        StateChanged?.Invoke();
    }

    public bool isOccupied()
    {
        return gameState == GameState.OnPC || gameState == GameState.Inspecting/*|| gameState == GameState.DayOver*/ || gameState == GameState.InArchive;
    }

    public bool isFrozen()
    {
        return gameState == GameState.OnPC || gameState == GameState.Inspecting|| gameState == GameState.DayOver || gameState == GameState.Frozen || gameState == GameState.InArchive;
    }
    public GameState GetGameState()
    {
        return gameState;
    }
    public GameState GetPrevGameState()
    {
        return prevGameState;
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

    public string GetFeedBackExplanation()
    {
        return feedBackExplanation;
    }

    public void SaveData(SaveData data)
    {
        if (Resources.LoadAll<Case>(dayOrder[furthestDay]).Count() == 0)
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
        print("Loading day: " + furthestDay);
        LoadNewDay(furthestDay, true);
        PinboardBlocked = day < 2;
        DayIntro();
    }
    public bool GetAnswerCommited()
    {
        return answerCommited;
    }
    public void SetStartTransform(Transform t)
    {
        startPosition = t.position;
        startRotation = t.rotation;
        startCameraRotation = t.GetChild(0).transform.rotation;
    }
    public Vector3 GetStartPosition()
    {
        return startPosition;
    }
    public Quaternion GetStartRotation()
    {
        return startRotation;
    }

    public Quaternion GetStartCamRotation()
    {
        return startCameraRotation;
    }

    public Person[] GetPeople()
    {
        return people;
    }
    public ArchiveData[] GetArchiveData()
    {
        return archiveData;
    }


    public SocialMediaUser[] GetUsers()
    {
        return users;
    }

    public SocialMediaPost[] GetPosts()
    {
        return posts;
    }

    public DMConversation[] GetConversations()
    {
        return conversations;
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
            //DontDestroyOnLoad(gameObject);
            instance = this;
        }
        DontDestroyOnLoad(instance); 
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
        if (inspectionCam == null)
            inspectionCam = GameObject.Find("CalendarCam");
    }
    void Start()
    {
        print("Day: " + calendarLoad);
        if (calendarLoad)
        {  
            return;
        }
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        saveManager = SaveManager.instance;
        am = AudioManager.instance;
        SetStartTransform(GameObject.Find("Player").transform);

        if (Utility.CheckSaveFileExists(instance.saveFile.ToString()))
        {
            SaveManager.instance.SetupSaveFile(instance.saveFile.ToString());
            if (Utility.CheckSaveFileExists(instance.saveFile.ToString()))
            {
                SaveManager.instance.LoadGame();
            }
        }
        else
        {
            print("reeeal neeew game!!!");
          //  DayIntro();
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
    private bool LoadCase()
    {
        //TODO: ONLY FOR TESTING REMOVE AFTERWARDS
        if (devCase != 0)
        {
            day = devCase;
        }
        currentCase = Resources.LoadAll<Case>(dayOrder[day])[0];
        // load all connections
        connections = Resources.LoadAll<Connections>(dayOrder[day] + "/Connections");
        Mail[] tempMails = Resources.LoadAll<Mail>(dayOrder[day] + "/Mails");
        if (feedBackExplanation != "")
        {
           // Mail feedBackMail = Resources.Load<Mail>("FeedBackTemplate");
           // feedBackMail.message = feedBackExplanation;
           // mails = tempMails.Concat(new Mail[] { feedBackMail }).ToArray();
        }
        else
        {
            
        }
        mails = tempMails;
        // mails[tempMails.Count()] = feedBackMail;        
        posts = Resources.LoadAll<SocialMediaPost>(dayOrder[day] + "/Posts");
        conversations = Resources.LoadAll<DMConversation>(dayOrder[day] + "/Conversations");
        return false;
    }
    public IEnumerator delaySuspectClearing(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        currentlyAccused = null;
        currentInvestigationState = investigationStates.SuspectNotFound;
    }
    public void LoadNewDay(int day, bool loaded = false)
    {
        delaySuspectClearing(1);   

        instantiateLoadedDay = loaded;
        if (instantiatedDayIntro != null)
            Destroy(instantiatedDayIntro);
        GameObject ow = GameObject.Find("OutsideWorld");
        if (ow != null)
        {
            ow.GetComponent<Animator>().SetTrigger("NewDay");
        }
        this.day = day;
        StartCoroutine(dalayedPinboardBlocked(day == 1));
        bool over = LoadCase();
        if (over)
        {
            return;
        }
        //using lists to add new values dynamicly, afterwards convert to array, because it won't change and will be more performant
        List<Person> tempPeople = new List<Person>();
        List<SocialMediaUser> tempUsers = new List<SocialMediaUser>();
        print("Day: " + day);
        print("Case: " + currentCase);
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
        reload();
        if (calendarLoad && day != 1)
        {
            DayIntro();
        }
        calendarLoad = false;
    }
    public void reloadIfOver()
    {
        if (day == 5)//Resources.LoadAll<Case>(dayOrder[day]).Count()
        {
            Destroy(GameManager.instance.gameObject);
            // TODO: implement endgame  
            SaveManager.instance.DeleteSave(); 
            SceneManager.LoadScene(3);
            //  return;
            // todo: Only temp solution...
        }
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
        GameObject g = Instantiate(feedbackReportPrefab);
    }

    public void setNewDay(bool firstDay = false)
    { 
        SetGameState(GameState.DayOver);   
        int pointsThisDay = investigationState == investigationStates.SuspectFound ? 100 : investigationState == investigationStates.SuspectSaved ? 0 : -100;
        currentInvestigationState = investigationState;
        answerCommited = false;
        if (!firstDay)
        {
            if (results.Count <= furthestDay && furthestDay == day)
            {
                print("added" + investigationState);
                results.Add(investigationState);
                firstTryResults.Add(investigationState);
            }
            else
            {
                results[day] = investigationState;
            } 

            if (day == 1)
            {
                narration.PlaySequence(investigationState == investigationStates.SuspectFound ? "firstDayFeedbackPositive" : "firstDayFeedbackNegative");
            }
            else
            {
                NextDaySequence();
            }
            if(investigationState == investigationStates.SuspectFound){
            day++;
             GameObject.Find("LastDayReportManager").GetComponent<LastDayReportManager>().Reset();
            }
            else
            {
                currentlyAccused = null;
            }
           
            if (day > furthestDay)
            {
                furthestDay = day;
            }
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
    public Person GetCurrentlyAccused()
    {
        return currentlyAccused;
    } 
    
    public investigationStates GetCurrentInvestigationState()
    {
        return currentInvestigationState;
    }
    
    public void checkSuspicionRemoved(Person p)
    {
        currentlyAccused = null;
        currentInvestigationState = investigationStates.SuspectNotFound;
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
        currentInvestigationState = investigationState;
        if (currentCase != null && currentCase.personReasoning != null && currentCase.personReasoning.Count != 0)
        {
            foreach (PersonReasoning pr in currentCase.personReasoning)
            {
                if (pr.person == currentlyAccused)
                {
                    feedBackExplanation = pr.reason;
                    break;
                }
            }
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
    public void LoadDayOverCalendar(int d)
    {
        calendarLoad = true;
        LoadNewDay(d);
        GameManager.instance.SetGameState(GameState.Playing);
        //DayIntro(0.5f);
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

    public void DayIntro(float delay = 0)
    {
        if (day != 1)
        {
            print("cool");
            StartCoroutine(DayIntroCoroutine(delay));
        }
        else
        {
            print("cringe");
             SetGameState(GameState.Playing);
        }
    } 
    public IEnumerator DayIntroCoroutine(float delay = 0)
    {
        print("delaay "+delay);
        yield return new WaitForSeconds(delay);
        while (GameObject.Find("Virtual Camera") == null)
        { 
        print("looking For cam ");
            yield return new WaitForSeconds(0.1f);
        } 
        
        SetGameState(GameState.Playing);
        GameObject instantiatedDayIntro = Instantiate(dayIntro);
        print("instantiated dayintro "+instantiatedDayIntro);
        instantiatedDayIntro.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Day " + day + ":";
        instantiatedDayIntro.transform.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = currentCase.caseName;
        instantiatedDayIntro.SetActive(true);
        //yield return new WaitForSeconds(4f);
        // instantiatedDayIntro.SetActive(false);
    }
    private IEnumerator DelayFirstDay()
    {
        if (day < 2)
        {
            PinboardBlocked = true;
        }
        LoadNewDay(day);
        yield return null;
    }
}

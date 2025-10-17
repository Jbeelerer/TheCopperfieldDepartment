using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Pinboard : MonoBehaviour
{
    Transform pinboardModel;

    [SerializeField] private Color contradictionColor;
    [SerializeField] private GameObject pinPrefab;

    [SerializeField] private GameObject thread;
    [SerializeField] private GameObject connectionPrefab;

    //  parentsOnPinboard contains the content and element of the parent element of the pinboard element. These are users and people. The children are posts, the transform of these are stored in subPins. The subPins are stored near their parent inside the defined zone.
    private Dictionary<ScriptableObject, PinboardElement> pinsOnPinboard = new Dictionary<ScriptableObject, PinboardElement>();
    // Contains all sub pins of a user or person, this is important for programmaticly adding new pins, so they are near their parent. Use Transform instead of Vector3, so the position will be automaticly update when moved.
    private Dictionary<ScriptableObject, List<Transform>> subPins = new Dictionary<ScriptableObject, List<Transform>>();

    private float zoneSizeX = 1.65f;
    private float zoneSizeY = 1.15f; 

    private float minSpaceBetweenPins = 0.5f;

    private Narration narration;

    [SerializeField] private Texture mysteriousPersonMaterial;
    [SerializeField] private Texture rightPenClickInfo;
    [SerializeField] private Texture leftPenClickInfo;
    [SerializeField] private Texture threadInfo;

    private bool firstLoad = true;

    private GameManager gm;
    public GameObject FlaggedThread { get; set; }
    public PinboardElement FlaggedPersonPin { get; set; }

    private Dictionary<ScriptableObject, GameObject> trashedPinboardElements = new Dictionary<ScriptableObject, GameObject>();

    [SerializeField] private ScriptableObject[] tutorialRelevantObjects;

    public void pinboardInteractions(bool t)
    {
        transform.Find("Block").gameObject.SetActive(t);
    }
    public void AddTrashedPin(ScriptableObject o, GameObject go)
    {
        trashedPinboardElements[o] = go;
    }

    public void ResetFlaggedPerson()
    {
        if (FlaggedPersonPin != null)
        {
            
        FlaggedPersonPin.SetAnnotationType(AnnotationType.None);
        FlaggedPersonPin = null;
        }
        if (FlaggedThread != null)
        {
             FlaggedThread = null;
        }
        
    }
    public void AddTutorialRelevantObjects()
    {
        foreach (ScriptableObject o in tutorialRelevantObjects)
        {
            if (subPins.ContainsKey(o) || pinsOnPinboard.ContainsKey(o))
            {
                continue;
            }
            AddPin(o);
        }
    }
    public string tutorialElementOnBoard()
    {
        Pinboard pinboard = FindObjectOfType<Pinboard>();
        int peopleOnBoard = 0;
        int postsOnBoard = 0;
        int usersOnBoard = 0;
        foreach (ScriptableObject so in tutorialRelevantObjects)
        {
            if (pinboard.pinsOnPinboard.ContainsKey(so))
            {
                if (so is SocialMediaPost)
                {
                    postsOnBoard++;
                }
                else if (so is Person)
                {
                    peopleOnBoard++;
                }
                else if (so is SocialMediaUser)
                {
                    usersOnBoard++;
                }
            }

        }
        if (peopleOnBoard < 2 && postsOnBoard == 0 && usersOnBoard == 0)
        {
            return "phoneReminderNothingAdded";
        }
        else if (peopleOnBoard < 2)
        {
            return "phoneReminderPersonNotAdded";
        }
        else if (postsOnBoard == 0 && usersOnBoard == 0)
        {
            return "phoneReminderPostNotAdded";
        }
        else
        {
            return "phoneCallIntro";
        }
    }
    public void AddConnectionIfExist(ScriptableObject from, ScriptableObject to, Transform thread)
    {
        Connections connection = gm.checkForConnectionText(from, to);
        if (connection != null)
        {
            LineRenderer lr = thread.GetComponent<LineRenderer>();
            PinboardElement fromElement = pinsOnPinboard[from];
            PinboardElement toElement = pinsOnPinboard[to];
            // if the connection already exists, do nothing, if only on one element remove the connection
            if (toElement.CheckIfConnected(connection) && fromElement.CheckIfConnected(connection))
            {
                return;
            }

            GameObject instance = Instantiate(connectionPrefab, thread);
            // handle contradiction color 
            //Color color;
            // UnityEngine.ColorUtility.TryParseHtmlString(connection.isContradiction ? "#F5867C" : "#F5DB7C", out color);
            instance.transform.Find("PostitNew").GetComponent<MeshRenderer>().material.color = connection.isContradiction ? contradictionColor : Color.white;
            instance.transform.position = thread.transform.GetChild(0).position - new Vector3(0, 0.05f, 0); 
            instance.GetComponentInChildren<TextMeshProUGUI>().text = connection.text;
            toElement.AddConnection(connection, instance.transform);
            fromElement.AddConnection(connection, instance.transform);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
        pinboardModel = transform.GetChild(0);
        //pinboardModel.localScale = new Vector3(pinboardModel.localScale.x * gm.GetCurrentCase().pinboardSize, pinboardModel.localScale.y * gm.GetCurrentCase().pinboardSize, pinboardModel.localScale.z);
        //instantiate a pin for the suspect    
        AddPin(null, new Vector3(0, 0, -pinboardModel.localScale.z / 2)); 
        AddPin(threadInfo, new Vector3( (pinboardModel.localScale.x / 3), 0, -pinboardModel.localScale.z / 2));   
        AddPin(rightPenClickInfo, new Vector3(0.5f - (pinboardModel.localScale.x / 2), 0.5f - (pinboardModel.localScale.y / 2), -pinboardModel.localScale.z / 2));
        AddPin(leftPenClickInfo, new Vector3(0.5f + minSpaceBetweenPins - (pinboardModel.localScale.x / 2), 0.5f - (pinboardModel.localScale.y / 2), -pinboardModel.localScale.z / 2));
        narration = FindObjectOfType<Narration>();  
        GameObject.FindObjectOfType<ComputerControls>().OnUnpinned.AddListener(RemoveByScriptableObject);
        gm.OnNewDay.AddListener(ResetPinboard);
        gm.InvestigationStateChanged.AddListener(AddAccusation);
    }
    public void AddAccusation()
    {
        PinboardElement mp = pinsOnPinboard.FirstOrDefault(x => x.Value.GetContent() == null).Value;
        Person p = gm.GetCurrentlyAccused();
        if (p == null)
        {
            return;
        }
        if (!pinsOnPinboard.ContainsKey(p))
        {
            AddPin(p);
        }
        mp.removeThreads();
        FlaggedThread = ConnectWithThread(pinsOnPinboard[p], mp).gameObject;
        FlaggedPersonPin = pinsOnPinboard[p];
    }

    private void RemoveByScriptableObject(ScriptableObject o)
    {
        if (pinsOnPinboard.ContainsKey(o))
        {
            pinsOnPinboard[o].DeleteElement();
        }
    }

    public void RemoveThingOnPinboardByElement(PinboardElement pe)
    {
        var keyOfValueToRemove = pinsOnPinboard.FirstOrDefault(x => x.Value == pe).Key;
        if (keyOfValueToRemove != null)
        {
            pinsOnPinboard.Remove(keyOfValueToRemove);
            // if the social media user has no posts, clear the subPins list completely 
            if (keyOfValueToRemove is Person || (keyOfValueToRemove is SocialMediaUser && subPins[keyOfValueToRemove].Count <= 1))
            {
                subPins.Remove(keyOfValueToRemove);
            }
            if (keyOfValueToRemove is SocialMediaPost)
            {
                // removing childeren not yet working correctly
                SocialMediaPost p = (SocialMediaPost)keyOfValueToRemove;
                subPins[p.author].Remove(pe.transform);
            }
            else if (subPins.ContainsKey(keyOfValueToRemove))
            {
                subPins[keyOfValueToRemove].Remove(pe.transform);
            }
        }
    }

    public void AddPin()
    {
        Instantiate(pinPrefab, transform).GetComponent<PinboardElement>();

    }
    public void AddPin(string text)
    {
        PinboardElement pinboardElement = Instantiate(pinPrefab, transform).GetComponent<PinboardElement>();
        pinboardElement.SetText(text);

    }
    public void AddPin(Texture image, Vector3 position)
    {
        PinboardElement pinboardElement = Instantiate(pinPrefab, transform).GetComponent<PinboardElement>();
        // first pin is the mysterious person, so it should not be deletable
        if (pinsOnPinboard.Count <= 0)
        {
            pinboardElement.MakeUndeletable();
        }
        pinboardElement.transform.localPosition = position;
        ScriptableObject o = ScriptableObject.CreateInstance("Person");
        pinsOnPinboard[o] = pinboardElement;
        pinboardElement.SetContent(image);
    }
    public void AddPin(ScriptableObject o)
    {
        if (pinsOnPinboard.ContainsKey(o))
            return;

        if (trashedPinboardElements.ContainsKey(o))
        {
            Destroy(trashedPinboardElements[o]);
            trashedPinboardElements.Remove(o);
        }

        List<Transform> takenPositions = new List<Transform>();

        PinboardElement pinboardElement = Instantiate(pinPrefab, transform).GetComponent<PinboardElement>();
        if (o is SocialMediaUser)
        {
            Transform canvas = pinboardElement.transform.Find("Canvas");
            Transform canvasCam = pinboardElement.transform.Find("CanvasCamera");
            canvas.localPosition += new Vector3(100, 0, 0);
            canvasCam.localPosition += new Vector3(100, 0, 0);

        }
        Vector3 positionOnGrid;
        if (pinsOnPinboard.Count <= 0)
        {
            pinboardElement.transform.localPosition = new Vector3(0, 0, -pinboardModel.localScale.z / 2); //+ new Vector3(pinboardModel.localScale.z / 2, (pinboardModel.localScale.y / 2) - (pinboardElement.transform.localScale.y / 2), 0);
        }
        else
        {
            if (o is not SocialMediaPost)
            {
                foreach (PinboardElement p in pinsOnPinboard.Values)
                {
                    if (p == null)
                    {
                        continue;
                    }
                    takenPositions.Add(p.transform);
                }
                foreach (List<Transform> l in subPins.Values)
                {
                    foreach (Transform t in l)
                    {
                        takenPositions.Add(t);
                    }
                }
                // If subPins contains the key, it has existed in the past and should be placed near its children and be connected with them
                if (subPins.ContainsKey(o) && subPins[o].Count > 0)
                {
                    // place user or person on pinboard, when children are already on the pinboard
                    float xPos = subPins[o].Average(x => x.localPosition.x);
                    float yPos = subPins[o].Average(x => x.localPosition.y);

                    if (xPos > 1)
                    {
                        xPos = subPins[o].Min(x => x.localPosition.x) - minSpaceBetweenPins;
                    }
                    else if (xPos < -1)
                    {
                        xPos = subPins[o].Max(x => x.localPosition.x) + minSpaceBetweenPins;
                    }
                    if (yPos > 0.2f)
                    {
                        yPos = subPins[o].Min(x => x.localPosition.y) - minSpaceBetweenPins;
                    }
                    else if (yPos < -0.2f)
                    {
                        yPos = subPins[o].Max(x => x.localPosition.y) + minSpaceBetweenPins;
                    }

                    pinboardElement.transform.localPosition = new Vector3(xPos, yPos, -pinboardModel.localScale.z / 2);
                    foreach (Transform t in subPins[o])
                    {
                        ConnectWithThread(pinboardElement, t.GetComponent<PinboardElement>());
                    }
                    subPins[o].Add(pinboardElement.transform);
                }
                else
                {
                    // place user or person on pinboard
                    Vector3 pos = GetPointWithGreatestDistanceToOtherPoints(Vector3.zero, pinboardModel.localScale.x, pinboardModel.localScale.y, takenPositions, zoneSizeY, zoneSizeX);
                    pinboardElement.transform.localPosition = pos;
                    subPins[o] = new List<Transform> { pinboardElement.transform };
                }
            }
            if (o is SocialMediaPost)
            {
                ScriptableObject so = ConversionUtility.Convert<SocialMediaPost>(o).author;
                if (!pinsOnPinboard.ContainsKey(so))
                {
                    AddPin(so);
                }
                Vector3 centerOfZone = pinsOnPinboard[so].transform.localPosition;
                float ySection = 0.2f;  //pinboardModel.localScale.y / 6;
                                        //float xSection = 1f;//pinboardModel.localScale.x / 3/2;  
                centerOfZone.x += zoneSizeX * (centerOfZone.x / pinboardModel.localScale.x);
                //centerOfZone.x += centerOfZone.x > xSection ? +zoneSizeX / 2 : centerOfZone.x < -xSection ? -zoneSizeX / 2 : 0;
                // go down by half of the zone size to get center of zone, since the user post is allways on top of a zone  
                centerOfZone.y += centerOfZone.y > ySection ? +zoneSizeY / 2 : centerOfZone.y < -ySection ? -zoneSizeY / 2 : 0;
                // For Y maybe the older version, on the line above, is better ASK ALEX
                // centerOfZone.y += zoneSizeY * (centerOfZone.y / pinboardModel.localScale.y);
                centerOfZone.z = 0;
                // ensure that the pin is still inside the pinboard
                centerOfZone.x = Mathf.Clamp(centerOfZone.x, (-pinboardModel.localScale.x / 2) + minSpaceBetweenPins / 2, (pinboardModel.localScale.x / 2) - minSpaceBetweenPins / 2);
                centerOfZone.y = Mathf.Clamp(centerOfZone.y, (-pinboardModel.localScale.y / 2) + minSpaceBetweenPins / 2, (pinboardModel.localScale.y / 2) - minSpaceBetweenPins / 2);
                // place post underneath user inside boundries       
                positionOnGrid = GetPointWithGreatestDistanceToOtherPoints(centerOfZone, zoneSizeX, zoneSizeY, subPins[so], minSpaceBetweenPins / 2, minSpaceBetweenPins / 2);
                // set the center of zone on top of the pinboard model, so it visible and not inside the model
                centerOfZone.z = -pinboardModel.localScale.z / 2;
                pinboardElement.transform.localPosition = positionOnGrid;
                subPins[so].Add(pinboardElement.transform);
                if (so == null)
                {
                    pinsOnPinboard.Remove(so);
                }
                else
                {
                    ConnectWithThread(pinboardElement, pinsOnPinboard[so]);
                }
            }
        }
        pinsOnPinboard[o] = pinboardElement;
        pinboardElement.SetContent(o);
        if (o is IPinnable temp)
        {
            if (temp.suspicious)
            {
                narration.Say("positiveFeedback");
            }
            else if (temp.notSuspicious)
            {
                narration.Say("negativeFeedback");
            }
        }
    }
    private LineRenderer ConnectWithThread(PinboardElement element1, PinboardElement element2)
    {
        // auto connect post to user by thread
        LineRenderer threadObject = Instantiate(thread, transform).GetComponent<LineRenderer>();
        element1.AddStartingThread(threadObject);
        element2.AddEndingThreads(threadObject);

        Vector3 pointA = element1.transform.GetChild(0).position;
        Vector3 pointB = element2.transform.GetChild(0).position;

        threadObject.positionCount = 4;
        threadObject.SetPosition(0, pointA);
        threadObject.SetPosition(1, pointA);
        threadObject.SetPosition(2, pointB);
        threadObject.SetPosition(3, pointB);

        MakeColliderMatchLineRenderer(threadObject, pointA, pointB);
        return threadObject;
    }

    // Gets the Point with the greatest distance to other points, to ensure a equal distribution of elements on the pinboard
    private Vector3 GetPointWithGreatestDistanceToOtherPoints(Vector3 relativePos, float xWidth, float yHeight, List<Transform> takenPositions, float ySpareSpace, float xSpareSpace)
    {
        Vector3 positionOnGrid = new Vector3(0, 0, -pinboardModel.localScale.z / 2);//startPos;
        Dictionary<Vector3, float> everyPos = new Dictionary<Vector3, float>();
        Vector3 closest = Vector3.zero;

        // Iterate through the entire pinboard in a grid and save the distance to the closest element
        for (float y = (yHeight / 2) - ySpareSpace; y > -(yHeight / 2) + ySpareSpace; y -= yHeight / 6)
        {
            positionOnGrid.y = y;
            for (float x = xSpareSpace - (xWidth / 2); x < (xWidth / 2) - xSpareSpace; x += xWidth / 10)
            {
                positionOnGrid.x = x;
                Vector3 repositioned = positionOnGrid + relativePos;
                foreach (Transform p in takenPositions)
                {
                    Vector3 takenPos = p.localPosition;
                    if (closest == Vector3.zero)
                        closest = p.position;
                    closest = (Vector3.Distance(repositioned, p.localPosition) < Vector3.Distance(repositioned, closest)) ? p.localPosition : closest;
                }
                everyPos.Add(repositioned, Vector3.Distance(repositioned, closest));
            }
        }
        return everyPos.Count == 0 ? Vector3.zero : everyPos.FirstOrDefault(x => x.Value == everyPos.Values.Max()).Key;
    }
    public void MakeColliderMatchLineRenderer(LineRenderer lr, Vector3 pointA, Vector3 pointB)
    {
        pointB.x = pointA.x;
        BoxCollider collider = lr.GetComponentInChildren<BoxCollider>();
        //make collider match linerenderer

        // Set the position to be the midpoint between A and B
        collider.transform.position = (pointA + pointB) / 2;
        // put the collider on the surface of the pinboard
        collider.transform.localPosition = new Vector3(collider.transform.localPosition.x, collider.transform.localPosition.y, -pinboardModel.localScale.z / 2);

        // Set the scale's y-component to be the distance between A and B   
        collider.transform.localScale = new Vector3(0.01f, Vector3.Distance(pointA, pointB), 0.1f);

        // Rotate the collider to align with the line from A to B
        Vector3 direction = pointB - pointA;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        collider.transform.rotation = rotation;
    }

    public void ResetPinboard()
    {
        if (firstLoad)
        {
            firstLoad = false;
            // return;
        }
        List<PinboardElement> toDelete = new List<PinboardElement>();
        foreach (PinboardElement p in pinsOnPinboard.Values)
        {
            toDelete.Add(p);
        }
        foreach (PinboardElement p in toDelete)
        {
            p.DeleteElement();
        }
        pinsOnPinboard.Clear();
        subPins.Clear();
        foreach (GameObject go in trashedPinboardElements.Values)
        {
            Destroy(go);
        }
        trashedPinboardElements.Clear();
        // find all threads(Clone) and destroy them
        foreach (Transform child in transform)
        {
            if (child.name == "thread(Clone)")
            {
                Destroy(child.gameObject);
            }
        }
        AddPin(mysteriousPersonMaterial, new Vector3(0, 0, -pinboardModel.localScale.z / 2));
    }
}


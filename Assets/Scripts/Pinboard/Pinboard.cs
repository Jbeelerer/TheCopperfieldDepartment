using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Pinboard : MonoBehaviour
{
    Transform pinboardModel;

    [SerializeField] private GameObject pinPrefab;

    [SerializeField] private GameObject thread;

    //  parentsOnPinboard contains the content and element of the parent element of the pinboard element. These are users and people. The children are posts, the transform of these are stored in subPins. The subPins are stored near their parent inside the defined zone.
    private Dictionary<ScriptableObject, PinboardElement> parentsOnPinboard = new Dictionary<ScriptableObject, PinboardElement>();
    // Contains all sub pins of a user or person, this is important for programmaticly adding new pins, so they are near their parent. Use Transform instead of Vector3, so the position will be automaticly update when moved.
    private Dictionary<ScriptableObject, List<Transform>> subPins = new Dictionary<ScriptableObject, List<Transform>>();

    private float zoneSizeX = 1.5f;
    private float zoneSizeY = 1.5f;

    private float minSpaceBetweenPins = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        //instantiate a pin for the suspect 
        pinboardModel = transform.GetChild(0);
        AddPin(new ScriptableObject());
    }

    public void RemoveThingOnPinboardByElement(PinboardElement pe)
    {
        var keyOfValueToRemove = parentsOnPinboard.FirstOrDefault(x => x.Value == pe).Key;
        if (keyOfValueToRemove != null)
        {
            subPins[pe.GetContent()].Remove(pe.transform);
            parentsOnPinboard.Remove(keyOfValueToRemove);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddPin(new Person());
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
    public void AddPin(ScriptableObject o)
    {
        if (parentsOnPinboard.ContainsKey(o))
            return;
        List<Transform> takenPositions = new List<Transform>();

        PinboardElement pinboardElement = Instantiate(pinPrefab, transform).GetComponent<PinboardElement>();

        Vector3 positionOnGrid;
        if (parentsOnPinboard.Count <= 0)
        {
            pinboardElement.transform.position = pinboardModel.position + new Vector3(pinboardModel.localScale.z / 2, (pinboardModel.localScale.y / 2) - (pinboardElement.transform.localScale.y / 2), 0);
        }
        else
        {

            if (o is not SocialMediaPost)
            {
                foreach (PinboardElement p in parentsOnPinboard.Values)
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
                if (subPins.ContainsKey(o))
                {
                    float averageX = subPins[o].Average(x => x.localPosition.x);
                    float highestY = subPins[o].Max(x => x.localPosition.y);
                    pinboardElement.transform.localPosition = new Vector3(averageX, highestY + minSpaceBetweenPins, -pinboardModel.localScale.z / 2);
                    foreach (Transform t in subPins[o])
                    {
                        ConnectWithThread(pinboardElement, t.GetComponent<PinboardElement>());
                    }
                    subPins[o].Add(pinboardElement.transform);
                }
                else
                {
                    // place user or person on pinboard
                    pinboardElement.transform.localPosition = GetPointWithGreatestDistanceToOtherPoints(Vector3.zero, pinboardModel.localScale.x, pinboardModel.localScale.y, takenPositions, zoneSizeY / 2, zoneSizeX / 2);
                    subPins[o] = new List<Transform> { pinboardElement.transform };
                }
                /*   
                TODO: Maybe add a feature, where the first few elements are placed nearer the center, so it looks less empty
                if (subPins.Count <= 2)
                   {
                       positionOnGrid = positionOnGrid / 3;
                       positionOnGrid.z = -pinboardModel.localScale.z / 2;
                   }*/
            }
            if (o is SocialMediaPost)
            {
                ScriptableObject so = ConversionUtility.Convert<SocialMediaPost>(o).author;
                Vector3 centerOfZone = parentsOnPinboard[so].transform.localPosition;
                // go down by half of the zone size to get center of zone, since the user post is allways on top of a zone
                centerOfZone.y -= zoneSizeY / 2;
                centerOfZone.z = 0;
                // place post underneath user inside boundries  
                positionOnGrid = GetPointWithGreatestDistanceToOtherPoints(centerOfZone, zoneSizeX, zoneSizeY, subPins[so], minSpaceBetweenPins, minSpaceBetweenPins);
                // set the center of zone on top of the pinboard model, so it visible and not inside the model
                centerOfZone.z = -pinboardModel.localScale.z / 2;
                pinboardElement.transform.localPosition = positionOnGrid;
                subPins[so].Add(pinboardElement.transform);
                if (so == null)
                {
                    parentsOnPinboard.Remove(so);
                }
                else
                {
                    ConnectWithThread(pinboardElement, parentsOnPinboard[so]);
                }
            }
        }
        parentsOnPinboard[o] = pinboardElement;
        pinboardElement.SetContent(o);
    }
    private void ConnectWithThread(PinboardElement element1, PinboardElement element2)
    {
        // auto connect post to user by thread
        LineRenderer threadObject = Instantiate(thread, transform).GetComponent<LineRenderer>();
        element1.AddStartingThread(threadObject);
        element2.AddEndingThreads(threadObject);

        Vector3 pointA = element1.transform.GetChild(0).position;
        Vector3 pointB = element2.transform.GetChild(0).position;

        threadObject.SetPosition(0, pointA);
        threadObject.SetPosition(1, pointB);

        MakeColliderMatchLineRenderer(threadObject, pointA, pointB);
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

        // Set the scale's y-component to be the distance between A and B
        collider.transform.localScale = new Vector3(0.01f, Vector3.Distance(pointA, pointB), 0.1f);

        // Rotate the collider to align with the line from A to B
        Vector3 direction = pointB - pointA;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        collider.transform.rotation = rotation;
    }
}


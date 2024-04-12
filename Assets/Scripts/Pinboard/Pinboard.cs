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

    // the key element describes the position of the pinboard element, the list are the positions of the pins inside the zone
    private Dictionary<ScriptableObject, PinboardElement> thingsOnPinboard = new Dictionary<ScriptableObject, PinboardElement>();
    private Dictionary<ScriptableObject, List<Vector3>> pinsOnPinboard = new Dictionary<ScriptableObject, List<Vector3>>();

    private float zoneSizeX = 1.5f;
    private float zoneSizeY = 1.5f;

    private float minSpaceBetweenPins = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        //instantiate a pin for the suspect 
        pinboardModel = transform.GetChild(0);
        AddPin(new ScriptableObject());
    }

    public void removeThingOnPinboardByElement(PinboardElement pe)
    {
        var keyOfValueToRemove = thingsOnPinboard.FirstOrDefault(x => x.Value == pe).Key;
        if (keyOfValueToRemove != null)
        {
            thingsOnPinboard.Remove(keyOfValueToRemove);
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
        if (thingsOnPinboard.ContainsKey(o))
            return;
        List<Vector3> takenPositions = new List<Vector3>();

        PinboardElement pinboardElement = Instantiate(pinPrefab, transform).GetComponent<PinboardElement>();

        //pinboardModel.localScale.z / 2 offset so its on top of pinboard
        Vector3 positionOnGrid = Vector3.zero;// pinboardModel.position + new Vector3(pinboardModel.localScale.z / 2, 0, 0);
        // First element is allways the suspect and should be on the top in the center
        if (thingsOnPinboard.Count <= 0)
        {
            pinboardElement.transform.position = pinboardModel.position + new Vector3(pinboardModel.localScale.z / 2, (pinboardModel.localScale.y / 2) - (pinboardElement.transform.localScale.y / 2), 0);
        }
        else
        {

            if (o is not SocialMediaPost)
            {
                foreach (PinboardElement p in thingsOnPinboard.Values)
                {
                    if (p == null)
                    {
                        continue;
                    }
                    takenPositions.Add(p.transform.localPosition);
                }

                // place user or person on pinboard
                positionOnGrid = GetPointWithGreatestDistanceToOtherPoints(Vector3.zero, pinboardModel.localScale.x, pinboardModel.localScale.y, takenPositions, zoneSizeY / 2, zoneSizeX / 2);
                if (pinsOnPinboard.Count <= 2)
                {
                    positionOnGrid = positionOnGrid / 3;
                    positionOnGrid.z = -pinboardModel.localScale.z / 2;
                }
                pinsOnPinboard[o] = new List<Vector3> { positionOnGrid };
                // positionOnGrid += pinboardModel.position + new Vector3(pinboardModel.localScale.z / 2, 0, 0);
                pinboardElement.transform.localPosition = positionOnGrid;
            }
            // auto connect post to user   
            if (o is SocialMediaPost)
            {
                ScriptableObject so = ConversionUtility.Convert<SocialMediaPost>(o).author;
                Vector3 centerOfZone = thingsOnPinboard[so].transform.localPosition;
                centerOfZone.y -= zoneSizeY / 2;
                centerOfZone.z = 0;
                print(centerOfZone);
                // place post underneath user inside boundries  
                positionOnGrid = GetPointWithGreatestDistanceToOtherPoints(centerOfZone, zoneSizeX, zoneSizeY, pinsOnPinboard[so], minSpaceBetweenPins, minSpaceBetweenPins);
                centerOfZone.z = -pinboardModel.localScale.z / 2;
                // positionOnGrid += centerOfZone; //new Vector3(0, centerOfZone.y, centerOfZone.z);
                pinsOnPinboard[so].Add(positionOnGrid);
                print(positionOnGrid);
                pinboardElement.transform.localPosition = positionOnGrid;
                if (so == null)
                {
                    thingsOnPinboard.Remove(so);
                }
                else
                {
                    // connect to this
                    LineRenderer threadObject = Instantiate(thread, transform).GetComponent<LineRenderer>();
                    thingsOnPinboard[so].AddStartingThread(threadObject);
                    pinboardElement.AddEndingThreads(threadObject);

                    Vector3 pointA = thingsOnPinboard[ConversionUtility.Convert<SocialMediaPost>(o).author].transform.GetChild(0).position;
                    Vector3 pointB = pinboardElement.transform.GetChild(0).position;

                    threadObject.SetPosition(0, pointA);
                    threadObject.SetPosition(1, pointB);

                    MakeColliderMatchLineRenderer(threadObject, pointA, pointB);
                }
            }
        }
        thingsOnPinboard[o] = pinboardElement;
        pinboardElement.SetContent(o);
    }

    private Vector3 GetPointWithGreatestDistanceToOtherPoints(Vector3 relativePos, float xWidth, float yHeight, List<Vector3> takenPositions, float ySpareSpace, float xSpareSpace)
    {
        Vector3 positionOnGrid = new Vector3(0, 0, -pinboardModel.localScale.z / 2);//startPos;
        Dictionary<Vector3, float> everyPos = new Dictionary<Vector3, float>();
        Vector3 closest = Vector3.zero;

        // das ganze pinboard in einem grid durchgehen und den nächsten post speichern
        for (float y = (yHeight / 2) - ySpareSpace; y > -(yHeight / 2) + ySpareSpace; y -= yHeight / 6)
        {
            //positionOnGrid = new Vector3(positionOnGrid.x, y, positionOnGrid.z);
            positionOnGrid.y = y;
            for (float x = xSpareSpace - (xWidth / 2); x < (xWidth / 2) - xSpareSpace; x += xWidth / 10)
            {
                // positionOnGrid = new Vector3(positionOnGrid.x, y, x);
                positionOnGrid.x = x;
                Vector3 repositioned = positionOnGrid + relativePos;
                foreach (Vector3 p in takenPositions)
                {
                    //normalizes position of element to grid
                    Vector3 takenPos = p;
                    if (closest == Vector3.zero)
                        closest = p;
                    closest = (Vector3.Distance(repositioned, p) < Vector3.Distance(repositioned, closest)) ? p : closest; //&& Vector3.Distance(positionOnGrid, p) > 1
                }
                everyPos.Add(repositioned, Vector3.Distance(repositioned, closest));
            }
        }

        /* //  Old code: closer while less than 7
        if (takenPositions.Count < 7)
        {
            List<float> sortedValues = everyPos.Values.ToList();
            sortedValues.Sort();
            int count = sortedValues.Count;
            key = count % 2 == 0 ? sortedValues[(count + 1) / 2] : sortedValues[count / 2];
        }*/

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


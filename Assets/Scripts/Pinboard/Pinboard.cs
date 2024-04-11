using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Pinboard : MonoBehaviour
{

    [SerializeField] private GameObject pinPrefab;

    private Dictionary<ScriptableObject, PinboardElement> thingsOnPinboard = new Dictionary<ScriptableObject, PinboardElement>();
    [SerializeField] private GameObject thread;


    // Start is called before the first frame update
    void Start()
    {
        //instantiate a pin for the suspect 
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
        foreach (PinboardElement p in thingsOnPinboard.Values)
        {
            if (p == null)
            {
                continue;
            }
            takenPositions.Add(p.transform.position);
        }
        Transform pinboardModel = transform.GetChild(0);
        //pinboardModel.localScale.z / 2 offset so its on top of pinboard
        Vector3 positionOnGrid = pinboardModel.position + new Vector3(pinboardModel.localScale.z / 2, 0, 0);
        Dictionary<Vector3, float> everyPos = new Dictionary<Vector3, float>();
        Vector3 closest = Vector3.zero;

        if (thingsOnPinboard.Count > 0)
        {

            // das ganze pinboard in einem grid durchgehen und den nächsten post speichern
            for (float y = pinboardModel.localScale.y; y > -(pinboardModel.localScale.y / 2); y -= pinboardModel.localScale.y / 6)
            {
                positionOnGrid = new Vector3(positionOnGrid.x, y, positionOnGrid.z);
                for (float x = -pinboardModel.localScale.x / 2; x < pinboardModel.localScale.x / 2; x += pinboardModel.localScale.x / 10)
                {
                    positionOnGrid = new Vector3(positionOnGrid.x, y, x);
                    foreach (Vector3 p in takenPositions)
                    {
                        if (closest == Vector3.zero)
                            closest = p;
                        closest = (Vector3.Distance(positionOnGrid, p) < Vector3.Distance(positionOnGrid, closest)) ? p : closest; //&& Vector3.Distance(positionOnGrid, p) > 1
                    }
                    everyPos.Add(positionOnGrid, Vector3.Distance(positionOnGrid, closest));
                }
            }
            float key;
            //   closer while less than 7
            if (takenPositions.Count < 7)
            {
                List<float> sortedValues = everyPos.Values.ToList();
                sortedValues.Sort();
                int count = sortedValues.Count;
                key = count % 2 == 0 ? sortedValues[(count + 1) / 2] : sortedValues[count / 2];
            }
            else
            {
                key = everyPos.Values.Max();
            }
            positionOnGrid = everyPos.FirstOrDefault(x => x.Value == key).Key + new Vector3(0, transform.position.y, transform.position.z);
        }
        PinboardElement pinboardElement = Instantiate(pinPrefab, transform).GetComponent<PinboardElement>();
        pinboardElement.transform.position = thingsOnPinboard.Count > 0 ? positionOnGrid : pinboardModel.position + new Vector3(pinboardModel.localScale.z / 2, 0, 0);
        // auto connect post to user   
        if (o is SocialMediaPost)
        {
            ScriptableObject so = ConversionUtility.Convert<SocialMediaPost>(o).author;
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
        thingsOnPinboard[o] = pinboardElement;
        pinboardElement.SetContent(o);
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


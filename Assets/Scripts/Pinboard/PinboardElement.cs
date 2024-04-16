using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PinboardElement : MonoBehaviour
{
    [SerializeField] private Material mysteriousPersonMaterial;

    // The reason for starting and ending threads, is that the lineRenderer has 0 and 1 for the start and endpoint respectively. This way the threads can be moved with the element.
    private List<LineRenderer> startingThreads = new List<LineRenderer>();
    private List<LineRenderer> endingThreads = new List<LineRenderer>();
    private List<PinboardElement> connectedElements = new List<PinboardElement>();

    private float animationTimer = 0;
    private float animationTime;
    private bool isMoving = false;

    private ScriptableObject content;

    private GameObject circle;
    private GameObject crossThrough;

    [SerializeField] private GameObject image;


    public ScriptableObject GetContent()
    {
        return content;
    }
    public void setIsMoving(bool b)
    {
        isMoving = b;
        if (!isMoving)
        {
            enableThreadCollider(true, startingThreads);
            enableThreadCollider(true, startingThreads);

            reApplyCollider(startingThreads);
            reApplyCollider(endingThreads);
        }
        else
        {
            enableThreadCollider(false, startingThreads);
            enableThreadCollider(false, endingThreads);
        }
    }
    // Enables or disables the collider of the threads and returns a list of still existing threads
    private List<LineRenderer> enableThreadCollider(bool b, List<LineRenderer> lineRenderers)
    {
        List<LineRenderer> cleanList = new List<LineRenderer>();
        foreach (LineRenderer l in lineRenderers)
        {
            if (l == null)
            {
                continue;
            }
            l.transform.GetComponentInChildren<Collider>().enabled = b;
            cleanList.Add(l);
        }
        return cleanList;
    }
    private void reApplyCollider(List<LineRenderer> lineRenderers)
    {
        List<LineRenderer> tempThreads = new List<LineRenderer>();
        Pinboard p = transform.GetComponentInParent<Pinboard>();
        foreach (LineRenderer l in lineRenderers)
        {
            if (l == null)
            {
                tempThreads.Add(l);
                continue;
            }
            l.transform.GetChild(0).gameObject.SetActive(true);
            if (lineRenderers == endingThreads)
            {
                l.SetPosition(1, transform.GetChild(0).position);
                l.SetPosition(2, transform.GetChild(0).position);
            }
            p.MakeColliderMatchLineRenderer(l, l.GetPosition(0), l.GetPosition(1));
            Transform connection = l.transform.Find("Connection(Clone)");
            if (connection)
            {
                connection.position = l.transform.GetChild(0).position - new Vector3(0, 0.05f, 0);
            }
        }

    }
    // Start is called before the first frame update
    public void AddStartingThread(LineRenderer l)
    {
        startingThreads.Add(l);
    }
    public void AddEndingThreads(LineRenderer l)
    {
        endingThreads.Add(l);
    }
    public void removeThreads()
    {
        foreach (LineRenderer l in startingThreads)
        {
            Destroy(l.gameObject);
        }
        foreach (LineRenderer l in endingThreads)
        {
            Destroy(l.gameObject);
        }
        endingThreads.Clear();
        startingThreads.Clear();
    }
    void Start()
    {
        circle = transform.GetChild(3).GetChild(0).gameObject;
        crossThrough = transform.GetChild(3).GetChild(1).gameObject;
        circle.SetActive(false);
        crossThrough.SetActive(false);
        animationTime = GetComponent<Animator>().GetAnimatorTransitionInfo(0).duration;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isMoving || animationTimer <= animationTime)
        {
            if (!isMoving)
                animationTime += Time.deltaTime;
            // TODO move threads
            List<LineRenderer> tempThreads = new List<LineRenderer>();
            foreach (LineRenderer l in startingThreads)
            {
                if (l == null)
                {
                    tempThreads.Add(l);
                    //startingThreads.Remove(l);
                    continue;
                }
                l.SetPosition(0, transform.GetChild(0).position);
                l.SetPosition(1, transform.GetChild(0).position);
            }
            removeThreads(tempThreads, startingThreads);
            foreach (LineRenderer l in endingThreads)
            {
                if (l == null)
                {
                    tempThreads.Add(l);
                    // endingThreads.Remove(l);
                    continue;
                }
                l.SetPosition(2, transform.GetChild(0).position);
                l.SetPosition(3, transform.GetChild(0).position);
            }
            removeThreads(tempThreads, startingThreads);
        }

    }

    private List<LineRenderer> removeThreads(List<LineRenderer> toRemove, List<LineRenderer> removeFrom)
    {
        foreach (LineRenderer l in toRemove)
        {
            removeFrom.Remove(l);
        }
        return removeFrom;
    }
    public void SetText(string text)
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }
    public void SetContent(ScriptableObject o)
    {
        TextMeshProUGUI textElement = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        content = o;
        switch (o)
        {
            case Person:
                Person person = ConversionUtility.Convert<Person>(o);
                textElement.text = person.personName;
                break;
            case SocialMediaPost:
                // connect to user
                image.SetActive(false);
                SocialMediaPost post = ConversionUtility.Convert<SocialMediaPost>(o);
                textElement.text = post.contentShort;
                break;
            case SocialMediaUser:

                SocialMediaUser user = ConversionUtility.Convert<SocialMediaUser>(o);
                textElement.text = user.username;
                textElement.verticalAlignment = VerticalAlignmentOptions.Bottom;
                //image.sprite = user.image;
                image.GetComponent<Renderer>().material.mainTexture = user.image.texture;
                break;
            default:
                // misterious person
                textElement.gameObject.SetActive(false);
                gameObject.transform.GetChild(2).GetComponent<Renderer>().material = mysteriousPersonMaterial;
                image.SetActive(false);
                transform.transform.GetChild(2).Rotate(new Vector3(0, 0, 180));
                break;
        }
    }
    public void DeleteElement()
    {
        foreach (PinboardElement p in connectedElements)
        {  // remove threads
            foreach (LineRenderer l in startingThreads)
            {
                p.DeleteThread(l);
            }
            foreach (LineRenderer l in endingThreads)
            {
                p.DeleteThread(l);
            }
        }
        foreach (LineRenderer l in startingThreads)
        {
            Destroy(l);
        }
        foreach (LineRenderer l in endingThreads)
        {
            Destroy(l);
        }
        GetComponentInParent<Pinboard>().RemoveThingOnPinboardByElement(this);
        Destroy(gameObject);
    }
    public void DeleteThread(LineRenderer l)
    {
        if (startingThreads.Contains(l))
        {
            startingThreads.Remove(l);
        }
        if (endingThreads.Contains(l))
        {
            endingThreads.Remove(l);
        }
    }
    public bool CheckIfCircleAnnotated()
    {
        return circle.activeSelf;
    }
    public bool CheckIfStrikeThroughAnnotated()
    {
        return crossThrough.activeSelf;
    }
    public void annotateCircle()
    {
        circle.SetActive(true);
        crossThrough.SetActive(false);
    }
    public void annotateStrikeThrough()
    {
        crossThrough.SetActive(true);
        circle.SetActive(false);
    }
    public void clearAnnotations()
    {
        crossThrough.SetActive(false);
        circle.SetActive(false);
    }
}

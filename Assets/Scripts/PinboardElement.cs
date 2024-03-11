using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PinboardElement : MonoBehaviour
{
    private List<LineRenderer> startingThreads = new List<LineRenderer>();
    private List<LineRenderer> endingThreads = new List<LineRenderer>();
    private bool isMoving = false;
    public void setIsMoving(bool b)
    {
        print("setting is moving" + b);
        isMoving = b;
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

    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            // TODO move threads
            foreach (LineRenderer l in startingThreads)
            {
                l.SetPosition(0, transform.position);
                print("setting position");
            }
            foreach (LineRenderer l in endingThreads)
            {
                l.SetPosition(1, transform.position);
                print("setting position");
            }
        }

    }
    public void SetText(string text)
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }
}

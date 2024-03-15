using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Pinboard : MonoBehaviour
{

    [SerializeField] private GameObject pinPrefab;

    private Dictionary<ScriptableObject, PinboardElement> thingsOnPinboard = new Dictionary<ScriptableObject, PinboardElement>();
    [SerializeField] private GameObject thread;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
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
        PinboardElement pinboardElement = Instantiate(pinPrefab, transform).GetComponent<PinboardElement>();
        if (o is SocialMediaPost)
        {
            // connect to this

            LineRenderer threadObject = Instantiate(thread, transform).GetComponent<LineRenderer>();
            thingsOnPinboard[ConversionUtility.Convert<SocialMediaPost>(o).author].AddStartingThread(threadObject);
            pinboardElement.AddEndingThreads(threadObject);

            threadObject.SetPosition(0, thingsOnPinboard[ConversionUtility.Convert<SocialMediaPost>(o).author].transform.GetChild(0).position);
            threadObject.SetPosition(1, pinboardElement.transform.GetChild(0).position);

        }
        thingsOnPinboard[o] = pinboardElement;
        pinboardElement.SetContent(o);
    }
}

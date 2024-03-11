using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinboard : MonoBehaviour
{

    [SerializeField] private GameObject pinPrefab;
    // Start is called before the first frame update
    void Start()
    {
        AddPin();
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
}

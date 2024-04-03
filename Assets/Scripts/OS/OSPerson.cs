using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSPerson : MonoBehaviour
{
    public Person person;

    private Pinboard pinboard;
    private OSPopupManager popupManager;

    // Start is called before the first frame update
    void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();
    }

    public void InstantiatePerson(Person person)
    {
        this.person = person;
    }

    public void AddPersonToPinboard()
    {
        pinboard.AddPin(person.personName);
        popupManager.DisplayPersonPinMessage();
    }

    public void AccusePerson()
    {
        popupManager.DisplayPersonDetainedMessage();
    }
}

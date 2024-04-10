using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OSPerson : MonoBehaviour
{
    public Person person;

    private Pinboard pinboard;
    private OSPopupManager popupManager;
    private GameManager gm;
    private FPSController fpsController;

    // Start is called before the first frame update
    void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();
        gm = GameManager.instance;
        fpsController = GameObject.Find("Player").GetComponent<FPSController>();

        fpsController.OnPinDeletion.AddListener(RemovePinned);
    }

    private void RemovePinned(ScriptableObject so)
    {
        switch (so)
        {
            case Person:
                if (so == person)
                {
                    transform.Find("PinPerson").GetComponent<Image>().color = Color.black;
                }
                break;
        }
    }

    public void InstantiatePerson(Person person)
    {
        this.person = person;
    }

    public void AddPersonToPinboard()
    {
        pinboard.AddPin(person);
        popupManager.DisplayPersonPinMessage();
        transform.Find("PinPerson").GetComponent<Image>().color = Color.red;
    }

    public void AccusePerson()
    {
        gm.checkSuspect(person);
        popupManager.DisplayPersonAccusedMessage();
        transform.Find("AccusePerson").GetComponent<Image>().color = Color.red;
    }
}

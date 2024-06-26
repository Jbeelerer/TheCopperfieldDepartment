using UnityEngine;
using UnityEngine.UI;

public class OSPerson : MonoBehaviour
{
    public Person person;

    private Pinboard pinboard;
    private OSPopupManager popupManager;
    private GameManager gm;
    private FPSController fpsController;
    private OSPeopleListContent peopleListContent;
    private ComputerControls computerControls;
    private bool personPinned = false;

    private void OnEnable()
    {
        if (person)
        {
            UpdateAccusation();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        popupManager = GameObject.Find("PopupMessage").GetComponent<OSPopupManager>();
        gm = GameManager.instance;
        fpsController = GameObject.Find("Player").GetComponent<FPSController>();
        peopleListContent = transform.GetComponentInParent<OSPeopleListContent>();
        computerControls = transform.GetComponentInParent<ComputerControls>();

        fpsController.OnPinDeletion.AddListener(RemovePinned);
        //peopleListContent.OnAccusedPersonClear.AddListener(ClearAccused);
        gm.InvestigationStateChanged.AddListener(UpdateAccusation);
    }

    public void InstantiatePerson(Person person)
    {
        this.person = person;
    }

    private void RemovePinned(ScriptableObject so)
    {
        switch (so)
        {
            case Person:
                if (so == person)
                {
                    transform.Find("PinPerson").GetComponent<Image>().color = Color.white;
                    personPinned = false;
                }
                break;
        }
    }

    public void AddPersonToPinboard()
    {
        // Pinning person
        if (!personPinned)
        {
            pinboard.AddPin(person);
            transform.Find("PinPerson").GetComponent<Image>().color = Color.red;
            personPinned = true;

            popupManager.DisplayPersonPinMessage();
        }
        // Unpinning person
        else
        {
            transform.Find("PinPerson").GetComponent<Image>().color = Color.white;
            computerControls.OnUnpinned?.Invoke(person);
            personPinned = false;

            popupManager.DisplayPersonUnpinMessage();
        }
    }

    public void AccusePerson()
    {
        if (!gm.checkIfPersonAccused(person))
        {
            computerControls.OpenWindow(OSAppType.WARNING, "You are about to accuse this person.<br><b>This can still be changed later.</b><br><br>Do you want to proceed?", AccusePersonSuccess);
        }
        else
        {
            ClearAccusation();
            popupManager.DisplayPersonUnaccusedMessage();
        }
    }

    public void AccusePersonSuccess()
    {
        gm.checkSuspect(person);
    }

    public void UpdateAccusation()
    {
        if (gm.checkIfPersonAccused(person))
        {
            //peopleListContent.ClearAccusedPeople();
            computerControls.investigationState = OSInvestigationState.PERSON_ACCUSED;
            if (computerControls.GetComponentInChildren<OSSocialMediaContent>())
            {
                computerControls.GetComponentInChildren<OSSocialMediaContent>().ClearDeletedPost();
            }
            popupManager.DisplayPersonAccusedMessage();
            transform.Find("AccusePerson").GetComponent<Image>().color = Color.red;
        }
        else
        {
            transform.Find("AccusePerson").GetComponent<Image>().color = Color.white;
        }
    }

    public void ClearAccusation()
    {
        gm.checkSuspicionRemoved(person);
    }
}

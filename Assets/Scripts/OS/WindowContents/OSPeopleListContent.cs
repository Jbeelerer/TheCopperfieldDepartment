using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OSPeopleListContent : MonoBehaviour
{
    [SerializeField] private GameObject profileContainer;
    [SerializeField] private GameObject personProfilePrefab;
    private int personNumber = 1;
    private readonly List<OSPerson> peopleList = new List<OSPerson>();
    private ComputerControls computerControls;

    //public UnityEvent OnAccusedPersonClear;

    private void Awake()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
    }

    public void LoadList(int day)
    {
        foreach (Person p in computerControls.GetPeople(day))
        {
            InstanciatePerson(p, day);
        }
    }

    /*private void OnEnable()
    {
        if (computerControls.investigationState == OSInvestigationState.POST_DELETED)
        {
            ClearAccusedPeople();
        }
    }*/

    public void InstanciatePerson(Person person, int day)
    {
        GameObject newProfile = Instantiate(personProfilePrefab, profileContainer.transform);
        newProfile.GetComponent<OSPerson>().InstantiatePerson(person);
        newProfile.name = "Profile" + personNumber;
        personNumber++;
        newProfile.transform.Find("ImageContainer").Find("Image").GetComponent<Image>().sprite = person.image;
        newProfile.transform.Find("name").GetComponent<TextMeshProUGUI>().text = person.personName;
        newProfile.transform.Find("info").GetComponent<TextMeshProUGUI>().text = person.description;

        var isCurrentDay = day == GameManager.instance.GetDay();
        newProfile.transform.Find("PinPerson").gameObject.SetActive(isCurrentDay);
        newProfile.transform.Find("AccusePerson").gameObject.SetActive(isCurrentDay);
        if (!isCurrentDay && person == GameManager.instance.GetCase(day).guiltyPerson)
        {
            newProfile.GetComponent<Image>().color = Color.lightGray;
            newProfile.transform.Find("GuiltyStamp").gameObject.SetActive(true);
        }
        peopleList.Add(newProfile.GetComponent<OSPerson>());
    }

    public void PinPerson(Person person)
    {
        peopleList.FirstOrDefault(p => p.person == person)?.AddPersonToPinboard();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OSPeopleListContent : MonoBehaviour
{
    [SerializeField] private GameObject profileContainer;
    [SerializeField] private GameObject personProfilePrefab;
    private int personNumber = 1;
    private List<OSPerson> peopleList = new List<OSPerson>();
    private ComputerControls computerControls;

    private void Awake()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
    }
    void Start()
    {
        Person[] people = Resources.LoadAll<Person>("People");
        foreach (Person p in people)
        {
            InstanciatePerson(p);
        }
    }

    private void OnEnable()
    {
        if (computerControls.investigationState == OSInvestigationState.POST_DELETED)
        {
            ClearAccusedPeople();
        }
    }

    public void InstanciatePerson(Person person)
    {
        GameObject newProfile = Instantiate(personProfilePrefab, profileContainer.transform);
        newProfile.GetComponent<OSPerson>().InstantiatePerson(person);
        //newProfile.GetComponent<OSSocialMediaPost>().instanctiatePost(post);
        newProfile.name = "Profile" + personNumber;
        personNumber++;
        newProfile.transform.Find("name").GetComponent<TextMeshProUGUI>().text = person.personName;
        newProfile.transform.Find("info").GetComponent<TextMeshProUGUI>().text = person.description;
        peopleList.Add(newProfile.GetComponent<OSPerson>());
    }

    public void ClearAccusedPeople()
    {
        foreach (OSPerson p in peopleList)
        {
            p.gameObject.transform.Find("AccusePerson").GetComponent<Image>().color = Color.black;
        }
    }
}

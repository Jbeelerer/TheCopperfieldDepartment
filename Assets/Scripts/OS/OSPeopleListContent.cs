using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSPeopleListContent : MonoBehaviour
{
    [SerializeField] private GameObject profileContainer;
    [SerializeField] private GameObject personProfilePrefab;
    private int personNumber = 1;

    // Start is called before the first frame update
    void Start()
    {
        Person[] people = Resources.LoadAll<Person>("People");
        foreach (Person p in people)
        {
            InstanciatePerson(p);
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
    }
}

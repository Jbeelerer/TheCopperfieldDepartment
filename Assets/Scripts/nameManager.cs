using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nameManager : MonoBehaviour
{
    string[] firstNames = new string[]
{
    // Male
    "James", "John", "Robert", "Michael", "William",
    "David", "Richard", "Joseph", "Thomas", "Charles",
    "Christopher", "Daniel", "Matthew", "Anthony", "Mark",
    "Donald", "Steven", "Paul", "Andrew", "Kenneth",
    "Kevin", "Brian", "George", "Edward", "Ronald",
    "Timothy", "Jason", "Jeffrey", "Ryan", "Gary",
    "Eric", "Stephen", "Jonathan", "Larry", "Justin",
    "Scott", "Brandon", "Benjamin", "Samuel", "Gregory",
    "Alexander", "Patrick", "Jack", "Dennis", "Jerry",
    "Tyler", "Aaron", "Adam", "Nathan", "Henry",
    "Douglas", "Peter", "Kyle", "Jeremy", "Walter",
    "Keith", "Roger", "Terry", "Austin", "Sean",
    "Gerald", "Carl", "Dylan", "Harold", "Jordan",
    "Jesse", "Bryan", "Lawrence",

    // Female
    "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth",
    "Barbara", "Susan", "Jessica", "Sarah", "Karen",
    "Nancy", "Lisa", "Betty", "Margaret", "Sandra",
    "Ashley", "Kimberly", "Emily", "Donna", "Michelle",
    "Dorothy", "Carol", "Amanda", "Melissa", "Deborah",
    "Stephanie", "Rebecca", "Laura", "Sharon", "Cynthia",
    "Kathleen", "Amy", "Shirley", "Angela", "Helen",
    "Anna", "Brenda", "Pamela", "Nicole", "Emma",
    "Samantha", "Katherine", "Christine", "Debra", "Rachel",
    "Carolyn", "Janet", "Catherine", "Maria", "Heather",
    "Diane", "Julie", "Joyce", "Victoria", "Kelly",
    "Christina", "Lauren", "Evelyn", "Judith", "Megan",
    "Cheryl", "Hannah", "Jacqueline", "Martha", "Gloria",
    "Teresa", "Ann", "Sara", "Frances", "Kathryn",
    "Alexis", "Beverly"
};

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public string newName()
    {
        PlayerPrefs.SetString("name", firstNames[Random.Range(0, firstNames.Length)]);
        return PlayerPrefs.GetString("name");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

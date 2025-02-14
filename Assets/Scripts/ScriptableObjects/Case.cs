using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Case", menuName = "ScriptableObjects/Case", order = 4)]
public class Case : ScriptableObject
{
    public int id;
    public ScriptableObject[] people;
    public ScriptableObject guiltyPerson;
    public ScriptableObject incriminatingPost;
    public List<PersonReasoning> personReasoning;
    public float pinboardSize = 0.8f;
    public string personFoundText = "Congratulations!! While apprehending the person you accused, we found damming evidence confirming the guilt!! ";
    public string personNotFoundText = "The person you accused was innocent! Your colleague was able to find the real culprit. This behavior is unacceptable, please be more careful next time";
    public string personSavedText = "We weren't able to find any posible culprit, we decided to close the case for the moment.";
}

[System.Serializable]
public class PersonReasoning
{
    public ScriptableObject person;
    public string reason;
}
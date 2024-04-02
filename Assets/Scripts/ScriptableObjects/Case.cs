using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Case", menuName = "ScriptableObjects/Case", order = 4)]
public class Case : ScriptableObject
{
    public int id;
    public ScriptableObject guiltyPerson;
    public ScriptableObject incriminatingPost;
}
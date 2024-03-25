using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SocialMediaUser", menuName = "ScriptableObjects/SocialMediaUser", order = 2)]
public class SocialMediaUser : ScriptableObject
{
    public int id;
    public string username;
    public Person realPerson;
    public Sprite image;
    public List<string> additionalInfos = new List<string>();
    public List<string> hiddenInfos = new List<string>();
}

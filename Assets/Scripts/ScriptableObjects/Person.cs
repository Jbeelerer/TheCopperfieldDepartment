using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Person", menuName = "ScriptableObjects/Person", order = 3)]
public class Person : ScriptableObject
{
    public int id;
    public string personName;
    public Sprite image;
    public SocialMediaUser socialMediaUser;
    // bool indicates
    public List<string> additionalInfos = new List<string>();
    public List<string> hiddenInfos = new List<string>();
}
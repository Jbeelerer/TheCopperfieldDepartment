using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Person", menuName = "ScriptableObjects/Person", order = 3)]
public class Person : ScriptableObject, IPinnable
{
    public int id;
    public string personName;
    public Sprite image;
    public SocialMediaUser socialMediaUser;
    public string description;
    // bool indicates
    public List<string> additionalInfos = new List<string>();
    public List<string> hiddenInfos = new List<string>();
    public Dictionary<ScriptableObject, string> connectionDescription = new Dictionary<ScriptableObject, string>();
    public bool suspicious;
    public bool notSuspicious;
    bool IPinnable.suspicious { get => suspicious; }
    bool IPinnable.notSuspicious { get => notSuspicious; }
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SocialMediaUser", menuName = "ScriptableObjects/SocialMediaUser", order = 2)]
public class SocialMediaUser : ScriptableObject, IPinnable
{
    public int id;
    public string username;
    public Person realPerson;
    public Sprite image;
    public List<string> additionalInfos = new List<string>();
    public List<string> hiddenInfos = new List<string>();
    public string bioText;
    public Sprite profileBanner;
    public bool suspicious;
    public bool notSuspicious;
    bool IPinnable.suspicious { get => suspicious; }
    bool IPinnable.notSuspicious { get => notSuspicious; }
}

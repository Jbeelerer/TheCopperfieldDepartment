using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SocialMediaPost", menuName = "ScriptableObjects/SocialMediaPost", order = 1)]
public class SocialMediaPost : ScriptableObject, IPinnable
{
    public int id;
    public string contentShort;
    [TextArea] public string content;
    public SocialMediaUser author;
    public string date;
    public string time;
    public Sprite image;
    public bool hiddenInHomeFeed;
    public bool suspicious;
    public bool notSuspicious;
    public SocialMediaUser containsPasswordOfUser;
    bool IPinnable.suspicious { get => suspicious; }
    bool IPinnable.notSuspicious { get => notSuspicious; }
}
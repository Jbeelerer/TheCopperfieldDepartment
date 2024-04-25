using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SocialMediaPost", menuName = "ScriptableObjects/SocialMediaPost", order = 1)]
public class SocialMediaPost : ScriptableObject
{
    public int id;
    public string contentShort;
    public string content;
    public SocialMediaUser author;
    public string date;
    public Sprite image;
    public bool hiddenInHomeFeed;

}
using UnityEngine;

[CreateAssetMenu(fileName = "SocialMediaPost", menuName = "ScriptableObjects/SocialMediaPost", order = 1)]
public class SocialMediaPost : ScriptableObject
{
    public int id;
    public string content;
    public string author;
    public string date;
    public string image;
}
using UnityEngine;

[CreateAssetMenu(fileName = "SocialMediaUser", menuName = "ScriptableObjects/SocialMediaUser", order = 2)]
public class SocialMediaUser : ScriptableObject
{
    public int id;
    public string username;
    public Person realPerson;
    public Sprite image;
}
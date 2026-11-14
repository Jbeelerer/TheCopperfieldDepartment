using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomDayDescription
{
    public int day;
    [TextArea] public string customDescription;
}

[CreateAssetMenu(fileName = "Person", menuName = "ScriptableObjects/Person", order = 3)]
public class Person : ScriptableObject, IPinnable
{
    public int id;
    public string personName;
    public Sprite image;
    public SocialMediaUser socialMediaUser;
    [TextArea] public string nextDayBriefing;
    public string description;
    [Header("Overwrite default description on certain days")]
    public CustomDayDescription[] customDayDescriptions;
    // bool indicates
    public List<string> additionalInfos = new List<string>();
    public List<string> hiddenInfos = new List<string>();
    public Dictionary<ScriptableObject, string> connectionDescription = new Dictionary<ScriptableObject, string>();
    public bool suspicious;
    public bool notSuspicious;
    public bool isSmall;
    bool IPinnable.isSmall { get => isSmall; }

    bool IPinnable.suspicious { get => suspicious; }
    bool IPinnable.notSuspicious { get => notSuspicious; }

    public Person()
    {
        id = -1;
        personName = "name";
        image = null;
        socialMediaUser = null;
        description = "Description";
        additionalInfos = new List<string>();
        hiddenInfos = new List<string>();
        connectionDescription = new Dictionary<ScriptableObject, string>();
        suspicious = false;
        notSuspicious = false;
    }
}
using System.Collections.Generic;
using UnityEngine;

public enum ArchiveType
{
    File,
    Text,
    Image,
}

[CreateAssetMenu(fileName = "Archive", menuName = "ScriptableObjects/Archive", order = 3)]
public class ArchiveData : ScriptableObject, IPinnable
{
    public int id;
    public int startDay;
    public int endDay;
    public ArchiveType type;
    public string archivename;
    public Sprite image;
    public string date;
    [TextArea] public string content;
    [TextArea] public string contentShort;
    public bool suspicious; public bool notSuspicious;
    bool IPinnable.suspicious { get => suspicious; }
    bool IPinnable.notSuspicious { get => notSuspicious; }
}
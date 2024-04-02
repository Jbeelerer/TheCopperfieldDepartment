using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Connections", menuName = "ScriptableObjects/Connections", order = 2)]
public class Connections : ScriptableObject
{
    // as a seperate SCriptable Object, so that multiple froms to tos a possible since, it could be possible for multible people.
    public int id;
    public string text;
    public ScriptableObject[] from;
    public ScriptableObject[] to;


}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mail", menuName = "ScriptableObjects/Mail", order = 5)]
public class Mail : ScriptableObject
{
    public int id;
    public string sender;
    public string title;
    [TextArea] public string message;
    public bool isMainCase;
}
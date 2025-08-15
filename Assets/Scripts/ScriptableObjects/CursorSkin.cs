using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "CursorSkin", menuName = "ScriptableObjects/CursorSkin", order = 6)]
public class CursorSkin : ScriptableObject
{
    public int id;
    public Texture2D spritesheet;
    [TextArea] public string cursorName;
}

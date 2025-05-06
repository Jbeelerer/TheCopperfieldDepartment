using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class DirectMessage
{
    public SocialMediaUser sender;
    public string message;
    public string timeStamp;
} 

[CreateAssetMenu(fileName = "DMConversation", menuName = "ScriptableObjects/DMConversation", order = 10)]
public class DMConversation : ScriptableObject
{
    public int id;
    public SocialMediaUser conversationMember1;
    public SocialMediaUser conversationMember2;
    public List<DirectMessage> messages;
}

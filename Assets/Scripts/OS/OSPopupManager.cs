using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSPopupManager : MonoBehaviour
{
    [SerializeField] private AudioClip notificationSound;

    private Animator anim;
    private TextMeshProUGUI messageText;
    private ComputerControls computerControls;
    private float timeOfLastMessage = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        messageText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        computerControls = GetComponentInParent<ComputerControls>();
    }

    public void DisplayPostPinMessage()=> DisplayMessage("Post added to pinboard!");

    public void DisplayUserPinMessage() => DisplayMessage("User added to pinboard!");

    public void DisplayPersonPinMessage() => DisplayMessage("Person added to pinboard!");

    public void DisplayPostUnpinMessage() => DisplayMessage("Removed Post Pin");

    public void DisplayUserUnpinMessage() => DisplayMessage("Removed User Pin");

    public void DisplayPersonUnpinMessage() => DisplayMessage("Removed Person Pin");

    public void DisplayPersonAccusedMessage() => DisplayMessage("Marked person as prime suspect!");

    public void DisplayPersonUnaccusedMessage() => DisplayMessage("Removed accusation");

    public void DisplayPostDeleteMessage() => DisplayMessage("Post flagged for deletion!");

    public void DisplayPostUndeleteMessage() => DisplayMessage("Deletion reverted.");

    public void DisplayAccountLoginMessage() => DisplayMessage("Login successful!");

    public void DisplayPasswordFoundMessage() => DisplayMessage("New password stored!");

    private void DisplayMessage(string msg)
    {
        messageText.text = msg;
        anim.Play("popupMessageSpawn", -1, 0);
        if (Time.time - timeOfLastMessage > 0.2f)
        {
            computerControls.audioManager.PlayAudio(notificationSound);
        }
        timeOfLastMessage = Time.time;
    }
}

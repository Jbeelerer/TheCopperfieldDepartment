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

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        messageText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        computerControls = GetComponentInParent<ComputerControls>();
    }

    public void DisplayPostPinMessage()
    {
        messageText.text = "Post added to pinboard!";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }

    public void DisplayUserPinMessage()
    {
        messageText.text = "User added to pinboard!";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }

    public void DisplayPersonPinMessage()
    {
        messageText.text = "Person added to pinboard!";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }

    public void DisplayPostUnpinMessage()
    {
        messageText.text = "Removed Post Pin";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }

    public void DisplayUserUnpinMessage()
    {
        messageText.text = "Removed User Pin";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }

    public void DisplayPersonUnpinMessage()
    {
        messageText.text = "Removed Person Pin";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }

    public void DisplayPersonAccusedMessage()
    {
        messageText.text = "Marked person as prime suspect!";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }

    public void DisplayPersonUnaccusedMessage()
    {
        messageText.text = "Removed accusation";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }

    public void DisplayPostDeleteMessage()
    {
        messageText.text = "Post flagged for deletion!";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }

    public void DisplayPostUndeleteMessage()
    {
        messageText.text = "Deletion reverted.";
        anim.Play("popupMessageSpawn", -1, 0);
        computerControls.audioManager.PlayAudio(notificationSound);
    }
}

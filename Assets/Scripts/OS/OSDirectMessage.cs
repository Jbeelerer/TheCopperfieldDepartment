using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OSDirectMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private GameObject imgContainer;
    [SerializeField] private TextMeshProUGUI timestamp;

    public void InstantiateDM(string messageText, string timestampText, Sprite img = null)
    {
        message.text = messageText;
        if (img != null)
        {
            // TODO: fix image display in prefab before using this feature

            //imgContainer.SetActive(true);
            //imgContainer.GetComponentInChildren<Image>().sprite = img;
        }
        timestamp.text = timestampText;
    }
}

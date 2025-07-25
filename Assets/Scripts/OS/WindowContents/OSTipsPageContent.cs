using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSTipsPageContent : MonoBehaviour
{
    [SerializeField] private MediaItem[] mediaItems;
    [SerializeField] private Transform mediaFileContainer;
    [SerializeField] private GameObject mediaFilePrefab;

    private void Start()
    {
        InitializeFiles();
    }

    public void InitializeFiles()
    {
        int itemCount = 0;
        foreach (MediaItem item in mediaItems)
        {
            itemCount++;
            var newMediaFile = Instantiate(mediaFilePrefab, mediaFileContainer);
            newMediaFile.name = "MediaFile" + itemCount;
            if (item.image != null)
            {
                newMediaFile.GetComponent<OSMediaFile>().Initialize(item.image);
            }
            else if (item.video != null)
            {
                newMediaFile.GetComponent<OSMediaFile>().Initialize(item.video);
            }
        }
    }
}

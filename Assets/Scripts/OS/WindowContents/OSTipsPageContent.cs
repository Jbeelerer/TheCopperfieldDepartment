using SaveSystem;
using System.Collections.Generic;
using UnityEngine;

public class OSTipsPageContent : MonoBehaviour, ISavable
{
    [SerializeField] private MediaItem[] mediaItems;
    [SerializeField] private Transform mediaFileContainer;
    [SerializeField] private GameObject mediaFilePrefab;

    private List<string> openedTips = new List<string>();

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
                newMediaFile.GetComponent<OSMediaFile>().Initialize(item.image, openedTips.Contains(item.image.name));
            }
            else if (item.video != null)
            {
                newMediaFile.GetComponent<OSMediaFile>().Initialize(item.video, openedTips.Contains(item.video.name));
            }
        }
    }

    public void AddOpenedTip(string name)
    {
        if (!openedTips.Contains(name))
        {
            openedTips.Add(name);
        }
    }

    // TODO: opened tips are not correctly loaded yet, because this window doesnt exist when loading is called
    public void LoadData(SaveData data)
    {
        openedTips = data.openedTips;
    }

    public void SaveData(SaveData data)
    {
        data.openedTips = openedTips;
    }
}

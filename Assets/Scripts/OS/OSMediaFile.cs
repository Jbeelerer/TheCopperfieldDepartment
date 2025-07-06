using TMPro;
using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class MediaItem
{
    public Sprite image;
    public VideoClip video;
}

public class OSMediaFile : MonoBehaviour
{
    [HideInInspector] public Sprite imageMedia;
    [HideInInspector] public VideoClip videoMedia;

    [SerializeField] private TextMeshProUGUI fileName;

    private ComputerControls computerControls;

    // Start is called before the first frame update
    void Start()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
    }

    public void Initialize(Sprite image)
    {
        imageMedia = image;
        fileName.text = image.name;
    }

    public void Initialize(VideoClip video)
    {
        videoMedia = video;
        fileName.text = video.name;
    }

    public void OpenMediaInWindow()
    {
        if (imageMedia)
        {
            computerControls.OpenWindow(OSAppType.IMAGE, imageFile: imageMedia);
        }
        else
        {
            computerControls.OpenWindow(OSAppType.IMAGE, videoFile: videoMedia);
        }
    }
}

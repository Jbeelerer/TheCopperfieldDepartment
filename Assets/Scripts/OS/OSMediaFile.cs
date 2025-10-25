using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private Image newIcon;

    private ComputerControls computerControls;
    private OSTipsPageContent tipsPageContent;

    // Start is called before the first frame update
    void Start()
    {
        computerControls = transform.GetComponentInParent<ComputerControls>();
        tipsPageContent = transform.GetComponentInParent<OSTipsPageContent>();
    }

    public void Initialize(Sprite image, bool alreadyOpened)
    {
        imageMedia = image;
        fileName.text = image.name;
        //if (!alreadyOpened) ShowNewIcon();
    }

    public void Initialize(VideoClip video, bool alreadyOpened)
    {
        videoMedia = video;
        fileName.text = video.name;
        //if (!alreadyOpened) ShowNewIcon();
    }

    private void ShowNewIcon()
    {
        newIcon.gameObject.SetActive(true);
    }

    public void OpenMediaInWindow()
    {
        if (imageMedia)
        {
            computerControls.OpenWindow(OSAppType.IMAGE, imageFile: imageMedia);
            tipsPageContent.AddOpenedTip(imageMedia.name);
        }
        else
        {
            computerControls.OpenWindow(OSAppType.IMAGE, videoFile: videoMedia);
            tipsPageContent.AddOpenedTip(videoMedia.name);
        }
        newIcon.gameObject.SetActive(false);
    }
}

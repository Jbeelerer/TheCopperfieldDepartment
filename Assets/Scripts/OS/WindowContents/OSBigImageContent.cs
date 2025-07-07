using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using UnityEngine.Video;

public class OSBigImageContent : MonoBehaviour
{
    [HideInInspector] public SocialMediaPost relatedPost;

    private Image image;
    private VideoPlayer videoPlayer;
    private RawImage videoTexture;

    void Awake()
    {
        image = transform.GetComponentInChildren<Image>();
        videoPlayer = transform.GetComponentInChildren<VideoPlayer>();
        videoTexture = transform.GetComponentInChildren<RawImage>();
        videoPlayer.gameObject.SetActive(false);
        videoTexture.gameObject.SetActive(false);
    }

    public void Setup(SocialMediaPost imagePost)
    {
        relatedPost = imagePost;
        image.sprite = imagePost.image;

        if (relatedPost.imageInspectionAreaContainer != null)
        {
            Instantiate(relatedPost.imageInspectionAreaContainer, image.transform);
        }
    }

    public void Setup(Sprite imageFile)
    {
        image.sprite = imageFile;
    }

    public void Setup(VideoClip videoFile)
    {
        image.gameObject.SetActive(false);
        videoPlayer.gameObject.SetActive(true);
        videoTexture.gameObject.SetActive(true);
        videoPlayer.clip = videoFile;
        videoPlayer.targetTexture.Release();
        videoPlayer.targetTexture.width = (int)videoFile.width;
        videoPlayer.targetTexture.height = (int)videoFile.height;
    }
}

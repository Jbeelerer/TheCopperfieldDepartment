using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
    }
}

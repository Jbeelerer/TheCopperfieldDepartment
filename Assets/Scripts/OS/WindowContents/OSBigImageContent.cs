using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSBigImageContent : MonoBehaviour
{
    [HideInInspector] public SocialMediaPost relatedPost;

    private Image image;

    void Awake()
    {
        image = transform.GetComponentInChildren<Image>();
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
}

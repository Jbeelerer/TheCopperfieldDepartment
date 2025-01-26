using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OSBigImageContent : MonoBehaviour
{
    private Image image;

    void Awake()
    {
        image = transform.GetComponentInChildren<Image>();
    }

    public void SetImage(Sprite img)
    {
        image.sprite = img;
    }
}

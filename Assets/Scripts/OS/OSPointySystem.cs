using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OSPointySystem : MonoBehaviour
{
    [SerializeField] private GameObject pointy;
    [SerializeField] private TextMeshProUGUI pointyText;

    private GameObject objectToClickNext;

    void Start()
    {
        
    }

    public void ShowPointy(Vector2 position, string message)
    {
        pointy.transform.position = position;
        pointyText.text = message;
    }
}

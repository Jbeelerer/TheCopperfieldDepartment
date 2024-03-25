using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSApplication : MonoBehaviour
{
    public OSAppType appType;

    [HideInInspector] public RectTransform recTrans;

    // Start is called before the first frame update
    void Start()
    {
        recTrans = GetComponent<RectTransform>();
    }
}

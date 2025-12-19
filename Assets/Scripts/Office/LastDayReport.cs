using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LastDayReport : MonoBehaviour
{
    private GameManager gm;
    [SerializeField] private TextMeshProUGUI suspectName;
    [SerializeField] private TextMeshProUGUI explenation;
    [SerializeField] private Image suspectImage;
    [SerializeField] private Image stamp;
    [SerializeField] private Sprite stampSuccess;
    [SerializeField] private Sprite stampFailed;
    [SerializeField] private GameObject newDayPrefab;
    [SerializeField] private GameObject test;
    [SerializeField] private RectTransform paper;
    public Texture2D canvasTexture;



    // Start is called before the first frame update
    void Awake()
    {
        gm = GameManager.instance;

        suspectName.text = gm.GetCurrentlyAccused().personName;
        suspectImage.sprite = gm.GetCurrentlyAccused().image;
        explenation.text = gm.GetFeedBackExplanation();
        print(gm.GetFeedBackExplanation()); 
        print(gm.GetDay()-1); 
        if (gm.GetResultForDay(gm.GetDay()) == investigationStates.SuspectFound)
        {
            stamp.sprite = stampSuccess; 
        }
        else
        {
            stamp.sprite = stampFailed;
            StartCoroutine(CaptureRectTransform(paper));
        }

    }
    private IEnumerator CaptureRectTransform(RectTransform rt)
{
    // Force layout and graphics to update
    Canvas.ForceUpdateCanvases();

    // Wait until the end of the frame to ensure everything is drawn
    yield return new WaitForEndOfFrame();

    // Get world corners
    Vector3[] corners = new Vector3[4];
    rt.GetWorldCorners(corners);

    // Convert to screen points
    Vector2 bl = RectTransformUtility.WorldToScreenPoint(null, corners[0]); // bottom-left
    Vector2 tr = RectTransformUtility.WorldToScreenPoint(null, corners[2]); // top-right

    // Ensure coordinates are in screen space and clamp to screen
    float x = Mathf.Clamp(Mathf.Min(bl.x, tr.x), 0, Screen.width);
    float y = Mathf.Clamp(Mathf.Min(bl.y, tr.y), 0, Screen.height);
    float width = Mathf.Clamp(Mathf.Abs(tr.x - bl.x), 1, Screen.width - x);
    float height = Mathf.Clamp(Mathf.Abs(tr.y - bl.y), 1, Screen.height - y);

    // Create the texture
    Texture2D tex = new Texture2D(Mathf.RoundToInt(width), Mathf.RoundToInt(height), TextureFormat.RGBA32, false);

    // Read pixels from screen
    Rect readRect = new Rect(x, y, width, height);
    tex.ReadPixels(readRect, 0, 0);
    tex.Apply();

    // Assign to material
    Material mat = new Material(test.GetComponent<Renderer>().material);
    mat.SetTexture("_SecondTexture", tex);
    test.GetComponent<Renderer>().material = mat;

    // Optional: save the texture if needed
    canvasTexture = tex;
    // find lastdayreportmanager and add last day report
    // find component LastDayReportManager

    print(GameObject.Find("LastDayReportManager"));
    print(GameObject.Find("LastDayReportManager").GetComponent<LastDayReportManager>());
    GameObject.Find("LastDayReportManager").GetComponent<LastDayReportManager>().AddLastDayReport(tex);
}



    public void Next()
    {
        GameObject g = Instantiate(newDayPrefab);
        GameManager.instance.reloadIfOver();
        Destroy(gameObject);
    }
}

using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class LastDayReport : MonoBehaviour
{
    private GameManager gm;
    private AudioManager am;
    [SerializeField] private TextMeshProUGUI suspectName;
    [SerializeField] private TextMeshProUGUI explenation;
    [SerializeField] private Image suspectImage;
    [SerializeField] private Image stamp;
    [SerializeField] private Sprite stampSuccess;
    [SerializeField] private Sprite stampFailed;
    [SerializeField] private GameObject test;
    [SerializeField] private RectTransform paper;
    public Texture2D canvasTexture;

    private bool continueAllowed = false;

    // Start is called before the first frame update
    void Awake()
    {
        gm = GameManager.instance;
        am = AudioManager.instance;

        // todo make sure the next day is instantiated after the last day report 
        bool isOver = GameManager.instance.reloadIfOver();
        if (!isOver)
        {
            print(gm.GetCurrentlyAccused().personName);
            suspectName.text = gm.GetCurrentlyAccused().personName;
            suspectImage.sprite = gm.GetCurrentlyAccused().image;
            explenation.text = gm.GetFeedBackExplanation();
            print(gm.GetFeedBackExplanation()); 
            print(gm.GetDay()-1); 
            if (gm.GetCurrentInvestigationState() == investigationStates.SuspectFound)
            {
                stamp.sprite = stampSuccess;
            }
            else
            {
                stamp.sprite = stampFailed;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
      //on click
        if (Input.GetMouseButtonDown(0) && continueAllowed)
        {
            StartCoroutine(Proceed());
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

    private IEnumerator Proceed()
    {
        if (gm.GetCurrentInvestigationState() == investigationStates.SuspectFound)
        {
            var timeMachine = FindFirstObjectByType<TimeMachine>();
            timeMachine.NewDayTransition();
        }
        else
        {
            yield return CaptureRectTransform(paper);
            gm.SetGameState(GameState.Playing);
        }

        GameManager.instance.StartDelaySuspectClearing(0.1f);
        GameObject.Find("Narration").GetComponent<Narration>().BlackScreenOff();
        GetComponentInChildren<Animator>().Play("ReportFadeOut");
    }

    //Used in ReportFadeOut animation event
    public void DestroyReportObject()
    {
        Destroy(transform.parent.gameObject);
    }

    //Used in ReportSlide animation event
    public void AllowContinue()
    {
        continueAllowed = true;
    }
    public void PlaySoundDuringAnimation(AudioClip clip)
    {
        if (am == null)
        {
            am = FindFirstObjectByType<AudioManager>();
        }
        am.PlayAudio(clip);
    }
}

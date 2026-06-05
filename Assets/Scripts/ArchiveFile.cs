using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchiveFile : MonoBehaviour
{
    private Animator anim;
    private GameManager gm;
    private Transform ParentTransform;
    private Vector3 originalPos;
    private ArchiveData data;
    [SerializeField] private Renderer fileImage;
    [SerializeField] private TMP_Text fileTitle;
    [SerializeField] private TMP_Text fileMapTitle;
    [SerializeField] private TMP_Text fileText;
    private bool isPinned = false;
    private GameObject canvas;

    //[SerializeField] private GameObject stickout;

    //private float stickoutStart = 0.375f;
    /*public void SetStickoutPosition(float x)
    {
        print("Setting stickout position to " + x);
        stickout.GetComponent<RectTransform>().localPosition = new Vector3(stickoutStart - (0.25f * (x / 2)), 0, -0.1f);
    }*/
    public ArchiveData GetData()
    {
        isPinned = true;
        return data;
    }
    // Start is called before the first frame update
    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
        anim = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

    }
    public string GetTitle() { return fileTitle.text; } 
    public void instantiateFile(ArchiveData d)
    {
        gm = FindFirstObjectByType<GameManager>();
        anim = GetComponent<Animator>();
        data = d;
        fileMapTitle.text = d.archivename;
        if (d.type != ArchiveType.Text)
        {
            fileImage.material.SetTexture("_Base", d.image.texture);
        }
        if (d.type != ArchiveType.Image)
        {
            fileTitle.text = d.archivename;
            fileText.text = d.content;
        }
        UpdateVisibility();
        gm.OnNewDay.AddListener(UpdateVisibility);
    }
    public void UpdateVisibility()
    {
        if (gm.GetCurrentCase() == null)
        {
            return;
        }
        print(data.startDay);
        print(data.endDay);
        print("day:" + gm.GetDay());

        print(gm.GetDay() <= data.endDay);
        print(gm.GetDay() >= data.startDay);
        if (gm.GetDay() >= data.startDay && gm.GetDay() <= data.endDay)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public void select()
    {
        anim.SetBool("selected", true);
    }
    public void deselect()
    {
        anim.SetBool("selected", false);
    }
    public void close()
    {
        anim.SetBool("open", false);
    }
    public void openFile()
    {
       // gm.InspectObject(transform, new Vector3(0, 1.5f, 2f), GameState.InArchive);
        //   data = d; 
        originalPos = transform.parent.position;    
        StartCoroutine(openFileAnim());
    }
    public void closeFile(Transform archive)
    {
        anim.SetBool("open", false);
       // gm.InspectObject(archive, new Vector3(0, 1.3f, 2.5f), GameState.InArchive);
        //   data = d;     
        StartCoroutine(closeFileAnim());
    }
  private IEnumerator openFileAnim()
{
    Vector3 startPos = transform.parent.position;
    Vector3 endPos = gm.GetCamPos().position - new Vector3(0, 0.8f, 0.8f);
    ParentTransform = transform.parent;
    
    float elapsedTime = 0f;
    float duration = 0.4f;  // <-- Change this to adjust speed (e.g., 0.5s is twice as fast)
    
    float amplitude = 0.5f; 
    float frequency = 2f; 
    
    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        
        // This creates a clean 0 to 1 percentage of completion
        float t = Mathf.Clamp01(elapsedTime / duration); 
        
        // The rest of your math now scales perfectly with the new speed
        float arc = Mathf.Sin(t * frequency * Mathf.PI * 0.5f) * amplitude;
        Vector3 interpolatedPos = Vector3.Lerp(startPos, endPos, t);
        interpolatedPos.y += arc;
        ParentTransform.position = interpolatedPos;
        
        yield return null;
    }
    
    anim.SetBool("open", true);
    ParentTransform.position = endPos;
}
    public IEnumerator closeFileAnim()
    {
        print("closing fr fr");
        Vector3 startPos = transform.parent.position;
        float elapsedTime = 0f;
        float duration = 0.4f;  // <-- Change this to adjust speed (e.g., 0.5s is twice as fast)
        
        float amplitude = 0.5f; 
        float frequency = 2f; 
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // This creates a clean 0 to 1 percentage of completion
            float t = Mathf.Clamp01(elapsedTime / duration);         
            // The rest of your math now scales perfectly with the new speed
            float arc = Mathf.Sin(t * frequency * Mathf.PI * 0.5f) * amplitude;
            Vector3 interpolatedPos = Vector3.Lerp(startPos, originalPos, t);
            interpolatedPos.y += arc;
            ParentTransform.position = interpolatedPos;
            if(anim.GetBool("open") == true && 0.2f > elapsedTime)
            { 
                anim.SetBool("open", false);
            } 
            yield return null;
        }
        ParentTransform.position = originalPos;
        print("now closed fr fr");
    }

    public void pinDoc()
    {
        canvas = GetComponentInChildren<Canvas>().gameObject;
        canvas.SetActive(false);   
    }
    
    public void unpinDoc()
    {
        canvas.SetActive(true);
    }
    


}

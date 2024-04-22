using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEditor;

public enum AnnotationType
{
    None,
    Circle,
    StrikeThrough
}
public class PinboardElement : MonoBehaviour
{
    [SerializeField] private Texture mysteriousPersonMaterial;

    // The reason for starting and ending threads, is that the lineRenderer has 0 and 1 for the start and endpoint respectively. This way the threads can be moved with the element.
    private List<LineRenderer> startingThreads = new List<LineRenderer>();
    private List<LineRenderer> endingThreads = new List<LineRenderer>();
    private List<PinboardElement> connectedElements = new List<PinboardElement>();

    private float animationTimer = 0;
    private float animationTime;
    private bool isMoving = false;

    private ScriptableObject content = null;

    private AnnotationType annotationType = AnnotationType.None;

    private GameObject image;

    [SerializeField] private GameObject[] postItMeshes;
    private GameObject postItMesh;

    private Coroutine waitingForContentToBeSet;

    public Texture2D canvasTexture;
    public Texture2D transparent;
    public Texture2D circle;
    public Texture2D strikeThrough;
    public Camera canvasCamera;

    public AnnotationType GetAnnotationType()
    {
        return annotationType;
    }

    public void SetAnnotationType(AnnotationType annotationType)
    {
        this.annotationType = annotationType;
        postItMesh.GetComponent<Renderer>().material.SetTexture("_AnnotationSprite", annotationType == AnnotationType.Circle ? circle : annotationType == AnnotationType.StrikeThrough ? strikeThrough : transparent);
    }

    public ScriptableObject GetContent()
    {
        return content;
    }
    public void setIsMoving(bool b)
    {
        isMoving = b;
        if (!isMoving)
        {
            enableThreadCollider(true, startingThreads);
            enableThreadCollider(true, startingThreads);

            reApplyCollider(startingThreads);
            reApplyCollider(endingThreads);
        }
        else
        {
            enableThreadCollider(false, startingThreads);
            enableThreadCollider(false, endingThreads);
        }
    }
    // Enables or disables the collider of the threads and returns a list of still existing threads
    private List<LineRenderer> enableThreadCollider(bool b, List<LineRenderer> lineRenderers)
    {
        List<LineRenderer> cleanList = new List<LineRenderer>();
        foreach (LineRenderer l in lineRenderers)
        {
            if (l == null)
            {
                continue;
            }
            l.transform.GetComponentInChildren<Collider>().enabled = b;
            cleanList.Add(l);
        }
        return cleanList;
    }
    private void reApplyCollider(List<LineRenderer> lineRenderers)
    {
        List<LineRenderer> tempThreads = new List<LineRenderer>();
        Pinboard p = transform.GetComponentInParent<Pinboard>();
        foreach (LineRenderer l in lineRenderers)
        {
            if (l == null)
            {
                tempThreads.Add(l);
                continue;
            }
            l.transform.GetChild(0).gameObject.SetActive(true);
            if (lineRenderers == endingThreads)
            {
                l.SetPosition(1, transform.GetChild(0).position);
                l.SetPosition(2, transform.GetChild(0).position);
            }
            p.MakeColliderMatchLineRenderer(l, l.GetPosition(0), l.GetPosition(1));
            Transform connection = l.transform.Find("Connection(Clone)");
            if (connection)
            {
                connection.position = l.transform.GetChild(0).position - new Vector3(0, 0.05f, 0);
            }
        }

    }
    // Start is called before the first frame update
    public void AddStartingThread(LineRenderer l)
    {
        startingThreads.Add(l);
    }
    public void AddEndingThreads(LineRenderer l)
    {
        endingThreads.Add(l);
    }
    public void removeThreads()
    {
        foreach (LineRenderer l in startingThreads)
        {
            Destroy(l.gameObject);
        }
        foreach (LineRenderer l in endingThreads)
        {
            Destroy(l.gameObject);
        }
        endingThreads.Clear();
        startingThreads.Clear();
    }
    void Awake()
    {
        postItMesh = Instantiate(postItMeshes[Random.Range(0, postItMeshes.Length - 1)], transform);
        image = postItMesh.transform.GetChild(0).gameObject;
        postItMesh = postItMesh.transform.GetChild(1).gameObject;
        postItMesh.transform.Rotate(new Vector3(Random.Range(-10, 10), 0, 0));
        //        postItMesh.transform.GetChild(0).Rotate(new Vector3(0, 0, Random.Range(-5, 5)));
        postItMesh.transform.SetAsLastSibling();
        animationTime = GetComponent<Animator>().GetAnimatorTransitionInfo(0).duration;

    }

    void Start()
    {
        RenderPipelineManager.endCameraRendering += this.OnEndCameraRendering;
    }
    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (this.waitingForContentToBeSet == null && camera == canvasCamera && camera != null && camera.gameObject != null)
            this.waitingForContentToBeSet = StartCoroutine(WaitForContentToBeSet(camera));
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (isMoving || animationTimer <= animationTime)
        {
            if (!isMoving)
                animationTime += Time.deltaTime;
            // TODO move threads
            List<LineRenderer> tempThreads = new List<LineRenderer>();
            foreach (LineRenderer l in startingThreads)
            {
                if (l == null)
                {
                    tempThreads.Add(l);
                    //startingThreads.Remove(l);
                    continue;
                }
                l.SetPosition(0, transform.GetChild(0).position);
                l.SetPosition(1, transform.GetChild(0).position);
            }
            removeThreads(tempThreads, startingThreads);
            foreach (LineRenderer l in endingThreads)
            {
                if (l == null)
                {
                    tempThreads.Add(l);
                    // endingThreads.Remove(l);
                    continue;
                }
                l.SetPosition(2, transform.GetChild(0).position);
                l.SetPosition(3, transform.GetChild(0).position);
            }
            removeThreads(tempThreads, startingThreads);
        }

    }

    private List<LineRenderer> removeThreads(List<LineRenderer> toRemove, List<LineRenderer> removeFrom)
    {
        foreach (LineRenderer l in toRemove)
        {
            removeFrom.Remove(l);
        }
        return removeFrom;
    }
    public void SetText(string text)
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }
    private IEnumerator WaitForContentToBeSet(Camera camera)
    {
        // wait for content to be set
        while (content == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        if (content is Person || content is SocialMediaPost || content is SocialMediaUser)
        {
            int res = 300;
            // Res must be min the size of the screen, otherwise it will crash... because readpixles uses the screen size
            // to handle previous mentioned problem
            if (res > Screen.width || res > Screen.height)
            {
                res = Screen.width < Screen.height ? Screen.width : Screen.height;
            }
            canvasTexture = new Texture2D(res, res, TextureFormat.RGBA64, false);
            Rect regionToReadFrom = new Rect((Screen.width / 2) - (res / 2), (Screen.height / 2) - (res / 2), res, res);//new Rect((Screen.width / 2) - (Screen.height / 2), (Screen.height / 2) - (Screen.height / 2), Screen.height, Screen.height);//new Rect(canvasTexture.width / 2, 0, canvasTexture.width, canvasTexture.height);
            int xPosToWriteTo = 0;
            int yPosToWriteTo = 0;
            bool updateMipMapsAutomatically = false;
            canvasTexture.ReadPixels(regionToReadFrom, xPosToWriteTo, yPosToWriteTo, updateMipMapsAutomatically);
            canvasTexture.Apply();
            Material material = new Material(postItMesh.GetComponent<Renderer>().material);
            material.name = "PostItMaterial" + transform.position.x + transform.position.y;
            material.SetTexture("_SecondTexture", canvasTexture);
            postItMesh.GetComponent<Renderer>().material = material;     //delete camera and canvas after rendering and saving the texture   
            Destroy(camera.gameObject);
            Destroy(transform.GetChild(2).gameObject);//transform.Find("Canvas").gameObject);
                                                      // Put the code that you want to execute after the camera renders here
                                                      // If you are using URP or HDRP, Unity calls this method automatically
                                                      // If you are writing a custom SRP, you must call RenderPipeline.EndCameraRendering
        }
    }
    public void SetContent(ScriptableObject o)
    {
        TextMeshProUGUI textElement = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        content = o;

        switch (o)
        {
            case Person:
                Person person = ConversionUtility.Convert<Person>(o);
                textElement.text = person.personName;
                break;
            case SocialMediaPost:
                // connect to user
                image.SetActive(false);
                SocialMediaPost post = ConversionUtility.Convert<SocialMediaPost>(o);
                textElement.text = post.contentShort;
                break;
            case SocialMediaUser:

                SocialMediaUser user = ConversionUtility.Convert<SocialMediaUser>(o);
                textElement.text = user.username;
                textElement.verticalAlignment = VerticalAlignmentOptions.Bottom;
                image.GetComponent<Renderer>().material.mainTexture = user.image.texture;
                break;
            default:
                // misterious person  
                postItMesh.GetComponent<Renderer>().material.SetTexture("_SecondTexture", mysteriousPersonMaterial);
                image.SetActive(false);
                //transform.transform.GetChild(2).Rotate(new Vector3(0, 0, 180));
                break;
        }
    }
    public void DeleteElement()
    {
        foreach (PinboardElement p in connectedElements)
        {  // remove threads
            foreach (LineRenderer l in startingThreads)
            {
                p.DeleteThread(l);
            }
            foreach (LineRenderer l in endingThreads)
            {
                p.DeleteThread(l);
            }
        }
        foreach (LineRenderer l in startingThreads)
        {
            Destroy(l);
        }
        foreach (LineRenderer l in endingThreads)
        {
            Destroy(l);
        }
        GetComponentInParent<Pinboard>().RemoveThingOnPinboardByElement(this);
        Destroy(gameObject);
    }
    public void DeleteThread(LineRenderer l)
    {
        if (startingThreads.Contains(l))
        {
            startingThreads.Remove(l);
        }
        if (endingThreads.Contains(l))
        {
            endingThreads.Remove(l);
        }
    }
}

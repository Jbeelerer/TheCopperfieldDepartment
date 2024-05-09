using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEditor;
using System.Diagnostics.CodeAnalysis;
using System;

public enum AnnotationType
{
    None,
    Circle,
    StrikeThrough
}
public class PinboardElement : MonoBehaviour
{
    private bool isDeletable = true;
    private bool hasInfo = true;
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

    private bool noStartThreadClipping = true;
    private bool noEndThreadClipping = true;

    public void MakeUndeletable()
    {
        isDeletable = false;
    }
    public bool GetIfDeletable()
    {
        return isDeletable;
    }
    public bool GetIfHasInfo()
    {
        return hasInfo;
    }
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
            enableThreadCollider(true, endingThreads);

            reApplyCollider(startingThreads);
            reApplyCollider(endingThreads);

            //Todo currently not implemented
            HandleAllThreadClipping();
        }
        else
        {
            noStartThreadClipping = true;
            noEndThreadClipping = true;
            enableThreadCollider(false, startingThreads);
            enableThreadCollider(false, endingThreads);
        }
        //Todo currently not implemented
        return;
        foreach (PinboardElement element in connectedElements)
        {
            element.HandleAllThreadClipping();
        }
    }

    public void HandleAllThreadClipping()
    {
        //todo this features is very ugly and not really working
        return;
        HandleThreadClipping(startingThreads);
        HandleThreadClipping(endingThreads);
    }

    public void ResetThreadClipping(bool isStart)
    {
        if (isStart)
        {
            noStartThreadClipping = true;
            foreach (LineRenderer l in startingThreads)
            {
                l.SetPosition(1, l.GetPosition(0));
            }
        }
        else
        {
            noEndThreadClipping = true;
            foreach (LineRenderer l in endingThreads)
            {
                l.SetPosition(2, l.GetPosition(3));
            }
        }
    }

    // todo handle the other side of the thread
    private void HandleThreadClipping(List<LineRenderer> lineRenderers)
    {
        bool startingThreads = lineRenderers == this.startingThreads;
        foreach (LineRenderer l in lineRenderers)
        {

            Vector3 start = startingThreads ? l.GetPosition(0) : l.GetPosition(3);
            Vector3 end = startingThreads ? l.GetPosition(3) : l.GetPosition(0);
            Vector3 dif = start - end;
            dif = dif.normalized;

            float zDistance = Math.Abs(Math.Abs(end.z) - Math.Abs(start.z));

            dif *= start.y > end.y + 0.5f ? 0.4f : 0.1f;
            float tempX = start.y > end.y + 0.5f && zDistance < 0.5f ? transform.position.x + 0.1f : zDistance > 0.1f ? transform.position.x + 0.07f : transform.position.x + 0.06f;
            if (start.y > end.y && zDistance < 0.8f)
            {
                noStartThreadClipping = false;
                noEndThreadClipping = false;

                l.SetPosition(startingThreads ? 1 : 2, new Vector3(tempX, start.y - dif.y, start.z - dif.z));
            }
            else
            {
                noStartThreadClipping = true;
                noEndThreadClipping = true;
            }
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
            l.transform.GetChild(0).gameObject.SetActive(b);
            // l.transform.GetComponentInChildren<Collider>().enabled = b;
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
            p.MakeColliderMatchLineRenderer(l, l.GetPosition(0), l.GetPosition(2));
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
        postItMesh = Instantiate(postItMeshes[UnityEngine.Random.Range(0, postItMeshes.Length - 1)], transform);
        image = postItMesh.transform.GetChild(0).gameObject;
        postItMesh = postItMesh.transform.GetChild(1).gameObject;
        postItMesh.transform.Rotate(new Vector3(UnityEngine.Random.Range(-10, 10), 0, 0));
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
                if (noStartThreadClipping)
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
                if (noEndThreadClipping)
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
            if (!hasInfo)
            {
                DestroyCameraAndCanvas();
                yield break;
            }
        }
        if (content is Person || content is SocialMediaPost || content is SocialMediaUser)
        {
            int res = Screen.height;
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
            material.SetColor("_MultiplicationColor", content is Person ? new Color(1, 1, 1, 1) : content is SocialMediaPost ? new Color(1, 0.8f, 1, 1) : new Color(1, 1, 0.8f, 1));
            postItMesh.GetComponent<Renderer>().material = material;
            //delete camera and canvas after rendering and saving the texture 
            DestroyCameraAndCanvas();
            //transform.Find("Canvas").gameObject);
            // Put the code that you want to execute after the camera renders here
            // If you are using URP or HDRP, Unity calls this method automatically
            // If you are writing a custom SRP, you must call RenderPipeline.EndCameraRendering
        }
        else
        {
            isDeletable = false;
        }
    }
    private void DestroyCameraAndCanvas()
    {
        Destroy(canvasCamera.gameObject);
        Destroy(transform.GetChild(2).gameObject);
    }
    // set Image as content
    public void SetContent(Texture t)
    {
        postItMesh.GetComponent<Renderer>().material.SetTexture("_SecondTexture", t);
        image.SetActive(false);
        content = null;
        hasInfo = false;
    }

    public void SetContent(ScriptableObject o)
    {
        TextMeshProUGUI textElement = gameObject.GetComponentInChildren<TextMeshProUGUI>();

        switch (o)
        {
            case Person:
                Person person = ConversionUtility.Convert<Person>(o);
                textElement.text = person.personName;
                textElement.verticalAlignment = VerticalAlignmentOptions.Bottom;
                image.GetComponent<Renderer>().material.mainTexture = person.image.texture;
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
                break;
        }
        content = o;
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

    public void HighlightElement(bool b)
    {
        if (b)
        {
            postItMesh.GetComponent<Renderer>().material.SetFloat("_Contrast", 1.6f);
        }
        else
        {
            postItMesh.GetComponent<Renderer>().material.SetFloat("_Contrast", 1f);
        }
    }
}

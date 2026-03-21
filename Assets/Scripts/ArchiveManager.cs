using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class ArchiveManager : MonoBehaviour
{
    private bool inUse = false;
    public bool fileOpen = false;
    public ArchiveFile currentSelection;

    private Archives currentArchive;

    private Pinboard pinboard;
    private GameManager gm;
    private bool isScrolling = false;
    private int currentFile = 0;
    private bool wasInArchvie = false;

    private Dictionary<ArchiveFile,GameObject> pinnedFiles = new Dictionary<ArchiveFile,GameObject>();

    //singleton pattern

    public static ArchiveManager Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetCurrentArchive(Archives archive)
    {
        currentArchive = archive;
        currentFile = 0;
        currentSelection = null;
        fileOpen = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        gm = FindFirstObjectByType<GameManager>();
        gm.StateChanged.AddListener(closeIfInArchive);
    }
    private void closeIfInArchive()
    {
        if (wasInArchvie && gm.GetGameState() == GameState.Playing)
        {
            wasInArchvie = false;
            currentArchive.close();
        }
        else if (gm.GetGameState() == GameState.InArchive)
        {
            wasInArchvie = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
            print(gm.GetGameState() );
        //shoot raycast from mouse
        if (gm != null && gm.GetGameState() == GameState.InArchive)
        {
            print("in archive!!!");
            // scroll through files
            //on any key down
            if (Input.anyKeyDown)
            {
                bool forward = false;
                if (Input.GetAxis("Vertical") > 0)
                {
                    currentFile = currentFile + 1 >= currentArchive.GetArchiveFiles().Count ? 0 : currentFile + 1;
                    forward = true;
                }
                if (Input.GetAxis("Vertical") < 0)
                {
                    currentFile = currentFile - 1 < 0 ? currentArchive.GetArchiveFiles().Count - 1 : currentFile - 1;
                }
             //   isScrolling = Input.GetAxis("Vertical") != 0;

                if (isScrolling)
                {
                    isScrolling = false;
                    if (currentSelection != null)
                        currentSelection.deselect();
                    int infiniteStopper = 0;
                    while (currentArchive.GetArchiveFiles()[currentFile].gameObject.activeSelf == false)
                    {
                        if (forward)
                        {
                            currentFile = currentFile + 1 >= currentArchive.GetArchiveFiles().Count ? 0 : currentFile + 1;
                        }
                        else
                        {
                            currentFile = currentFile - 1 < 0 ? currentArchive.GetArchiveFiles().Count - 1 : currentFile - 1;
                        }
                        infiniteStopper++;
                        if (infiniteStopper > currentArchive.GetArchiveFiles().Count + 1)
                        {
                            Debug.LogError("Infinite loop detected while scrolling through archive files.");
                            break;
                        }
                    }
                    currentSelection = currentArchive.GetArchiveFiles()[currentFile].gameObject.GetComponent<ArchiveFile>();
                    // gm.LookAt(currentSelection.transform);
                    currentSelection.select();
                }
                if (Input.GetButtonDown("Submit"))
                {
                    currentArchive.OpenArchiveFile();
                    //anim.SetBool("fileOpen", true);
                    SelectFile(currentArchive.GetArchiveFiles()[currentFile]);

                }
                currentArchive.SetCurrentSelection(currentSelection);
            }
           
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                print(hit.collider.gameObject.name);
                if (hit.collider.gameObject.tag == "ArchiveFile" && !fileOpen)
                {
                    if (currentSelection != hit.collider.gameObject.GetComponent<ArchiveFile>() && !isScrolling)
                    {
                        SelectFile(hit.collider.gameObject.GetComponent<ArchiveFile>());
                    }
                }
                else if(currentSelection != null && !fileOpen)
                {
                        currentSelection.deselect(); 
                        currentSelection = null;
                }
            
                if (Input.GetMouseButtonDown(0) && fileOpen && hit.collider.gameObject.name == "PinFile")
                {
                    if (currentSelection != null)
                    {
                        if (pinnedFiles.ContainsKey(currentSelection))
                        {
                        currentSelection.unpinDoc();
                        pinboard.RemoveByScriptableObject(currentSelection.GetData());
                        pinnedFiles.Remove(currentSelection);
                        return; 
                        }else{
                        pinboard.AddPin(currentSelection.GetData());
                        pinnedFiles.Add(currentSelection,currentSelection.gameObject);
                        currentSelection.pinDoc();
                        StartCoroutine(DelayedClosArchive());
                        currentSelection = null;
                        fileOpen = false;
                        return; }

                    }
                }
            }
            if (Input.GetMouseButtonDown(0) && currentSelection != null && !fileOpen)
            {
                fileOpen = true;
                currentArchive.OpenArchiveFile();
            }
            else if (Input.GetMouseButtonDown(0) && currentSelection != null && fileOpen)
            {
                fileOpen = false; 
                currentArchive.CloseArchiveFile();
            }

        }

    }

    // delayed so that the fps controller doesn't open the archive again
    public IEnumerator DelayedClosArchive()
    {
        yield return new WaitForSeconds(0.5f);
        currentArchive.close();
        gm.SetGameState(GameState.Playing);
        currentSelection = null;
    }
    public void SelectFile(ArchiveFile newFile)
    {
        if (currentSelection != null)
            currentSelection.deselect();
        currentSelection = newFile;
        // gm.LookAt(currentSelection.transform);
        currentSelection.select();
        currentArchive.SetCurrentSelection(currentSelection);
    }
}

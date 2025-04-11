using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Archives : MonoBehaviour
{
    [SerializeField] private GameObject[] files;
    [SerializeField] private GameObject filePrefab;
    [SerializeField] private GameObject fotoFilePrefab;
    [SerializeField] private GameObject textFilePrefab;
    [SerializeField] private List<ArchiveFile> archiveFiles = new List<ArchiveFile>();
    [SerializeField] private ArchiveData[] archiveData;
    private bool inUse = false;
    private bool fileOpen = false;
    private ArchiveFile currentSelection;

    private Pinboard pinboard;

    private Transform camPos;
    private Animator anim;
    private GameManager gm;
    private bool isScrolling = false;

    private int currentFile = 0;
    private bool wasInArchvie = false;
    // Start is called before the first frame update
    void Start()
    {
        float i = 0;
        float startPositionY = -5;
        foreach (ArchiveData fileData in archiveData)
        {
            GameObject fp = filePrefab;
            if (fileData.type == ArchiveType.Image)
            {
                fp = fotoFilePrefab;
            }
            else if (fileData.type == ArchiveType.Text)
            {
                fp = textFilePrefab;
            }

            GameObject newFile = Instantiate(fp, transform.GetChild(0));
            ArchiveFile f = newFile.GetComponentInChildren<ArchiveFile>();
            f.instantiateFile(fileData);
            f.SetStickoutPosition(i);
            archiveFiles.Add(f);
            newFile.transform.localPosition = new Vector3(0, 5, startPositionY + i);
            i += 2f;
        }

        anim = GetComponent<Animator>();
        camPos = transform.GetChild(1);
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        gm = FindObjectOfType<GameManager>();
        gm.StateChanged.AddListener(closeIfInArchive);
    }
    private void closeIfInArchive()
    {
        print("----- >>>" + gm.GetGameState());
        if (wasInArchvie && gm.GetGameState() == GameState.Playing)
        {
            wasInArchvie = false;
            close();
        }
        else if (gm.GetGameState() == GameState.InArchive)
        {
            wasInArchvie = true;
        }
    }

    public Transform getCamPos()
    {
        return camPos;
    }

    public void open()
    {
        print("open");
        anim.SetBool("open", true);
    }
    public void close()
    {
        anim.SetBool("open", false);
        if (currentSelection != null)
        {
            currentSelection.deselect();
            currentSelection.close();
            currentSelection = null;
        }
        fileOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        //shoot raycast from mouse
        if (gm != null && gm.GetGameState() == GameState.InArchive)
        {
            // scroll through files
            //on any key down
            if (Input.anyKeyDown)
            {
                if (Input.GetAxis("Vertical") > 0)
                {
                    currentFile = currentFile + 1 >= archiveFiles.Count ? 0 : currentFile + 1;
                }
                if (Input.GetAxis("Vertical") < 0)
                {
                    currentFile = currentFile - 1 < 0 ? archiveFiles.Count - 1 : currentFile - 1;
                }
                isScrolling = Input.GetAxis("Vertical") != 0;

                if (isScrolling)
                {
                    isScrolling = false;
                    if (currentSelection != null)
                        currentSelection.deselect();
                    currentSelection = archiveFiles[currentFile].gameObject.GetComponent<ArchiveFile>();
                    // gm.LookAt(currentSelection.transform);
                    currentSelection.select();
                }
                if (Input.GetButtonDown("Submit"))
                {
                    SelectFile(archiveFiles[currentFile]);
                }
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (Input.GetMouseButtonDown(0) && hit.collider.gameObject.name == "PinFile")
                {
                    if (currentSelection != null)
                    {
                        pinboard.AddPin(currentSelection.GetData());
                        //close(); 
                        gm.SetGameState(GameState.Playing);

                    }
                }
                if (hit.collider.gameObject.tag == "ArchiveFile" && !fileOpen)
                {
                    if (currentSelection != hit.collider.gameObject.GetComponent<ArchiveFile>() && !isScrolling)
                    {
                        SelectFile(hit.collider.gameObject.GetComponent<ArchiveFile>());
                    }
                }
            }
            if (Input.GetMouseButtonDown(0) && currentSelection != null && !fileOpen)
            {
                currentSelection.openFile();
                fileOpen = true;
            }

        }

    }
    public void SelectFile(ArchiveFile newFile)
    {
        if (currentSelection != null)
            currentSelection.deselect();
        currentSelection = newFile;
        // gm.LookAt(currentSelection.transform);
        currentSelection.select();
    }
}

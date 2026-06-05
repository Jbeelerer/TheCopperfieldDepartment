using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class Archives : MonoBehaviour
{
    [SerializeField] private GameObject[] files;
    [SerializeField] private GameObject filePrefab;
    [SerializeField] private GameObject locked;
     private bool isLocked = true;
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
    public string categoryName = "";
    private bool wasInArchvie = false;
    // Start is called before the first frame update
    public List<ArchiveFile> GetArchiveFiles()
    {
        return archiveFiles;
    }

    public string GetCurrentCategory()
    {
        return categoryName;
    }

    public bool GetIsLocked()
    {
        return isLocked;
    }

    public void SetCurrentSelection(ArchiveFile file)
    {
        currentSelection = file;
    }
    void Start()
    {
        GameManager.instance.OnNewDay.AddListener(UpdateArchiveAvailability);
        float i = 0;
        float startPosition = 0.007f;
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
            categoryName = fileData.categoryName;
            f.instantiateFile(fileData);
           // f.SetStickoutPosition(i);
            archiveFiles.Add(f);
            newFile.transform.localEulerAngles = new Vector3(180,90,90);
            newFile.transform.localScale = new Vector3(0.0004f,0.0004f,0.0004f);
            newFile.transform.localPosition = new Vector3(startPosition+i, 0.0002f, -0.0019f);
            i -= 0.0015f;
        }
        anim = GetComponent<Animator>();
        camPos = transform.GetChild(1);
        pinboard = GameObject.Find("Pinboard").GetComponent<Pinboard>();
        gm = FindFirstObjectByType<GameManager>();
        print("archivees" + i);
        StartCoroutine(waitForCaseLoad());
    }
    private IEnumerator waitForCaseLoad()
    {
        while (GameManager.instance.GetCurrentCase() == null)
        { 
            yield return new WaitForSeconds(0.1f);
        }
        UpdateArchiveAvailability();
    } 

    private void UpdateArchiveAvailability()
    {
        int count = 0;
        print(transform.GetChild(0));
        foreach (Transform child in transform.GetChild(0))
        {
            if (child.GetComponentInChildren<ArchiveFile>() != null)
            {
                count++;
            }
        }
        print(count);
        isLocked = count <= 0; 
        locked.SetActive(isLocked); 
        print(categoryName + " ... " + count);
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
            anim.SetBool("fileOpen", false);
            currentSelection = null;
        }
        fileOpen = false;
    }

    // Update is called once per frame
    public void OpenArchiveFile()
    {
        fileOpen = true;
        anim.SetBool("fileOpen", true);
        currentSelection.openFile();
    }
    public void CloseArchiveFile()
    {
        anim.SetBool("fileOpen", false);
        currentSelection.closeFile(transform);
        fileOpen = false;
    }
    // delayed so that the fps controller doesn't open the archive again
    public IEnumerator DelayedClosArchive()
    {
        print("closing: " + fileOpen + currentSelection);
        if (currentSelection != null && fileOpen)
        {
             yield return currentSelection.closeFileAnim();   
        }
        else
        {
        yield return new WaitForSeconds(0.5f);
        }
        print("done closing");
        close();
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
    }
}

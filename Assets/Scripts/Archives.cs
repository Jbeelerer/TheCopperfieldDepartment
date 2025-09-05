using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Archives : MonoBehaviour
{
    [SerializeField] private GameObject[] files;
    [SerializeField] private GameObject filePrefab;
    [SerializeField] private GameObject locked;
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
    public List<ArchiveFile> GetArchiveFiles()
    {
        return archiveFiles;
    }

    public void SetCurrentSelection(ArchiveFile file)
    {
        currentSelection = file;
    }
    void Start()
    {
        GameManager.instance.OnNewDay.AddListener(UpdateArchiveAvailability);
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

    private void UpdateArchiveAvailability()
    {
        locked.SetActive(!GameManager.instance.GetCurrentCase().hasArchived);
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
            anim.SetBool("fileOpen", false);
            currentSelection = null;
        }
        fileOpen = false;
    }

    // Update is called once per frame
    public void OpenArchiveFile()
    {
        anim.SetBool("fileOpen", true);
        currentSelection.openFile();
        fileOpen = true;
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
        yield return new WaitForSeconds(0.5f);
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

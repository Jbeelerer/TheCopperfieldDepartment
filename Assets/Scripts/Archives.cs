using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class Archives : MonoBehaviour
{
    [SerializeField] private GameObject[] files;
    [SerializeField] private Material lockedSprite;
    [SerializeField] private Material unlockedSprite;
    [SerializeField] private Material keySprite;
    [SerializeField] private GameObject screen;
    [SerializeField] private GameObject filePrefab;
    [SerializeField] private Transform lockHinge;
    [SerializeField] private Transform key;
    [SerializeField] private int startDay;
     private bool isLocked = true;
     private bool keyUsed = false;
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

    [SerializeField] private CharacterJoint joint;
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

    public bool GetIsKeyUsed()
    {
        return keyUsed;
    }
    public void UnlockArchive()
    {
        StartCoroutine(DelayUnlock(0.5f));
    }

    public string GetArchiveName()
    {
        return categoryName;
    }
      public Vector3 startPos;
    public Vector3 endPos;
    public float startRot;
    public float endRot;
    public float duration;

    private int DayChecked = 0;

    private IEnumerator DelayUnlock(float duration) // Changed 'time' to 'duration' to match your loop
    {
        float elapsedTime = 0f; // Make sure this is initialized to 0
        // Smoothly interpolate using Quaternions directly for cleaner rotation handling
        Vector3 startPos = key.localPosition;        
        Vector3 endPos = key.localPosition + new Vector3(0f, -0.5f, 0f); // Rotates 45 degrees on the local Y axis
        key.gameObject.SetActive(true);
          while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            key.localPosition = Vector3.Lerp(startPos, endPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Quaternion startRot = lockHinge.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 90, 0f); // Rotates 45 degrees on the local Y axis

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration; // Mathf.Clamp01 isn't strictly necessary if the while condition catches it, but it's safe
                        lockHinge.localRotation = Quaternion.Lerp(startRot, endRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        lockHinge.localRotation = endRot;
        // set material
        screen.GetComponent<Renderer>().material = unlockedSprite;
        Destroy(joint);
        yield return new WaitForSeconds(0.8f);
        keyUsed = true;
    } 
    public void UpdateScreen(Grabbable grabbable)
    {
        if(grabbable == null && !keyUsed)
        {
            screen.GetComponent<Renderer>().material = lockedSprite;
        }
        else if (grabbable != null && grabbable.GetKey() == categoryName)
        {
            screen.GetComponent<Renderer>().material = keySprite;
        }
    }
  
    public void SetCurrentSelection(ArchiveFile file)
    {
        currentSelection = file;
    }
    void Start()
    {
        PlayerPrefs.SetInt("archiveLocked_"+categoryName,0);
        FindFirstObjectByType<FPSController>().pickupEvent.AddListener(UpdateScreen);
        GameManager.instance.OnNewDay.AddListener(UpdateArchiveAvailabilityCoroutineManager);
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
        
    }
    private void UpdateArchiveAvailabilityCoroutineManager()
    {
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
        isLocked = startDay > GameManager.instance.GetDay(); 
        print(isLocked);
        print(startDay);
        print("----"+GameManager.instance.GetDay());

        if (!isLocked && startDay <= GameManager.instance.GetDay())
        {
            print("LOOCKED"+isLocked); 

            DayChecked = GameManager.instance.GetDay();
            if(startDay == GameManager.instance.GetDay())
            {
                GameManager.instance.SpawnKey(categoryName);
                PlayerPrefs.SetInt("archiveLocked_"+categoryName,1);
            }
            else
            { 
                key.localPosition = key.localPosition + new Vector3(0f, -0.5f, 0f); ;
                lockHinge.localRotation = lockHinge.localRotation* Quaternion.Euler(0f, 90, 0f);
                screen.GetComponent<Renderer>().material = unlockedSprite;
                Destroy(joint);
                keyUsed = true;
            }
        }
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

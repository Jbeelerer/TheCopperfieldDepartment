using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchiveFile : MonoBehaviour
{
    private Animator anim;
    private GameManager gm;
    private ArchiveData data;
    private bool isPinned = false;

    [SerializeField] private GameObject stickout;

    private float stickoutStart = 0.45f;
    public void SetStickoutPosition(float x)
    {
        stickout.GetComponent<RectTransform>().localPosition = new Vector3(stickoutStart - (0.15f * (x / 2)), 0, -0.1f);
    }
    public ArchiveData GetData()
    {
        isPinned = true;
        return data;
    }
    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void instantiateFile(ArchiveData d)
    {
        data = d;
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
        anim.SetBool("open", true);
        gm.InspectObject(transform, new Vector3(0, 1, 1f), GameState.InArchive);
        //   data = d;     
    }
}

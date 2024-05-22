using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashedPostIt : MonoBehaviour
{
    private ScriptableObject content;
    private Pinboard pinboard;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    public ScriptableObject GetContent()
    {
        return content;
    }
    public void SetContent(ScriptableObject content)
    {
        pinboard = FindObjectOfType<Pinboard>();
        this.content = content;
        pinboard.AddTrashedPin(content, gameObject);
    }
    public void ReAddPostIt()
    {
        pinboard.AddPin(content);
        Destroy(gameObject);
    }
}

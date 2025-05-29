using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WerkschauReseting : MonoBehaviour
{
    public float timer = 0;
    private float resetTime = 200;
    public GameObject display;
    private bool timerReached = false;
    private Transform player;
    private GameObject loadingScreen;
    private Vector3 startRotation;
    private Vector3 startPosition;
    // Start is called before the first frame update
    void Start()
    {
        display = transform.GetChild(0).gameObject;
        loadingScreen = transform.GetChild(1).gameObject;
        player = GameObject.Find("Player").transform;
        display.SetActive(false);
        startPosition = player.position;
        startRotation = player.rotation.eulerAngles;
        loadingScreen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetStartConditions();
        }
        if (timer >= resetTime && !timerReached)
        {
            SetStartConditions();
        }
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            timer = 0;
            if (timerReached)
            {
                timerReached = false;
                display.SetActive(false);
                loadingScreen.SetActive(true);
                GameManager.instance.ResetGame();
                SceneManager.LoadScene("NewMainScene");
            }
        }

    }
    public void SetStartConditions()
    {
        print("reset");
        timer = 0;
        timerReached = true;
        display.SetActive(true);
    }
}

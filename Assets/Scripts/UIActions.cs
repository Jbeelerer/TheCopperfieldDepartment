using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIActions : MonoBehaviour
{
    GameObject loadingScreen;
    private void Start()
    {
        GameManager.instance.NextDaySequence();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        loadingScreen = GameObject.Find("LoadingScreen");
        loadingScreen.SetActive(false);

    }
    public void StartGame()
    {
        loadingScreen.SetActive(true);
        GameManager.instance.ResetGame();
        SceneManager.LoadScene("NewMainScene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaveSystem;
using UnityEngine.SceneManagement;

public class SaveUI : MonoBehaviour
{

    public GameObject saveDialog;
    string saveFile;

    private GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
        saveDialog.SetActive(false);
    }

    public void Save()
    {
        string saveFile = gm.GetSaveFile().ToString();
        print("save" + saveFile);
        this.saveFile = saveFile;
        DoSave();
    }

    public void DoSave()
    {
        SaveManager.instance.SetupSaveFile(saveFile);
        SaveManager.instance.SaveGame();
        saveDialog.SetActive(false);
    }

    public void CancelSave()
    {
        saveDialog.SetActive(false);
    }

    public void Load()
    {
        SceneManager.LoadScene("01Main");
    }
    public void resumeGame()
    {
        // GameManager.instance.togglePause();
    }
    public void restartRun()
    {
        // GameManager.instance.togglePause();
        //GameManager.instance.resetGameManager();
        SaveManager.instance.DeleteSave();
        SceneManager.LoadScene("01Main");
    }
    public void MainMenu()
    {
        //GameManager.instance.togglePause();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("00Start");
    }
}

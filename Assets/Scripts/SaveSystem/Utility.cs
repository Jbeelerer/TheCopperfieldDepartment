using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class Utility
{
    public static void DebugMessage()
    {
        Debug.Log("Hello there");
    }

    public static bool CheckSaveFileExists(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        return File.Exists(filePath);
    }

    public static void ChangeScene(string name)
    {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
}

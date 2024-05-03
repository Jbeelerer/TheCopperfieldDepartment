using UnityEngine;
using UnityEngine.UI;

public class LoadBtnChecker : MonoBehaviour
{
    public string saveFile;

    void Start() {
        GetComponent<Button>().interactable = Utility.CheckSaveFileExists(saveFile);
    }

    public void CheckSaveFile(string saveFile) {
        GetComponent<Button>().interactable = Utility.CheckSaveFileExists(saveFile);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetTimer : MonoBehaviour
{
    [SerializeField] private float minutesToWait = 3f;
    [SerializeField] private GameObject warningMessage;

    private float lastInputTime;
    private bool messageActive;

    void Start()
    {
        lastInputTime = Time.time;
        ToggleMessage(false);
    }

    private void Update()
    {
        CheckTimerHotkeys();

        if (HasPlayerInput())
        {
            //print("input made");
            lastInputTime = Time.time;
            ToggleMessage(false);
        }

        if (!messageActive && Time.time - lastInputTime > (minutesToWait * 60 - 10))
        {
            ToggleMessage(true);
        }
        else if (Time.time - lastInputTime > minutesToWait * 60)
        {
            SceneManager.LoadScene("TitleScene");
        }
    }

    private void ToggleMessage(bool setActive)
    {
        messageActive = setActive;
        warningMessage.SetActive(setActive);
    }

    private bool HasPlayerInput()
    {
        if (Input.anyKeyDown)
            return true;

        if (Input.mousePositionDelta.sqrMagnitude > 0f)
            return true;

        if (Input.GetMouseButton(0) ||
            Input.GetMouseButton(1) ||
            Input.GetMouseButton(2))
            return true;

        if (Input.mouseScrollDelta.sqrMagnitude > 0f)
            return true;

        return false;
    }
    private void CheckTimerHotkeys()
    {
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                minutesToWait = i;
                print("Inactivity timer set to " + i + " minutes");
            }
        }
    }
}

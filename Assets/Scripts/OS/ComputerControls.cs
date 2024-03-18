using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum OSAppType
{
    SOCIAL,
    GOV,
    SETTINGS
}

public class ComputerControls : MonoBehaviour
{
    public float speed = 1;
    public RectTransform cursor;
    public OSWindow testWindow;
    public GameObject windowPrefab;
    public List<OSApplication> apps = new List<OSApplication>();

    private RectTransform screen;
    private float h;
    private float v;
    private List<OSWindow> windows = new List<OSWindow>();

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        screen = GetComponent<RectTransform>();
        windows.Add(testWindow);
    }

    // Update is called once per frame
    void Update()
    {
        h = speed * Input.GetAxis("Mouse X");
        v = speed * Input.GetAxis("Mouse Y");

        if (Input.GetMouseButtonDown(0))
        {
            // Clicked window bar
            CheckWindowClicked();
            cursor.transform.SetAsLastSibling();
        }

        if (Input.GetMouseButtonUp(0))
        {
            foreach (OSWindow window in windows)
            {
                window.isMoving = false;
            }

            // Clicked an app
            CheckAppClicked();
            cursor.transform.SetAsLastSibling();
        }
    }

    private void FixedUpdate()
    {
        cursor.anchoredPosition += new Vector2(h, v);

        foreach (OSWindow window in windows)
        {
            if (window.isMoving)
            {
                window.MoveWindow(new Vector2(h, v));
            }
        }

        // Reverse the movement if the cursor went outside the screen
        if (!PointInsideRect(cursor.position, screen))
        {
            cursor.anchoredPosition -= new Vector2(h, v);

            foreach (OSWindow window in windows)
            {
                if (window.isMoving)
                {
                    window.MoveWindow(-(new Vector2(h, v)));
                }
            }
        }
    }

    private void CheckWindowClicked()
    {
        foreach (OSWindow window in windows)
        {
            if (PointInsideRect(cursor.position, window.topBar))
            {
                // Check if window is behind another element
                if (TargetIsInFront(window.topBar.gameObject))
                {
                    window.isMoving = true;
                    window.transform.SetAsLastSibling();
                    return;
                }
            }
        }
    }

    private void CheckAppClicked()
    {
        foreach (OSApplication app in apps)
        {
            if (PointInsideRect(cursor.position, app.recTrans))
            {
                // Check if icon is behind another element
                if (!TargetIsInFront(app.gameObject))
                {
                    return;
                }
                // Check if the window is already open
                foreach (OSWindow window in windows)
                {
                    if (window.appType == app.appType)
                    {
                        return;
                    }
                }
                GameObject newWindow = Instantiate(windowPrefab, transform.position, transform.rotation, transform);
                newWindow.GetComponent<OSWindow>().appType = app.appType;
                windows.Add(newWindow.GetComponent<OSWindow>());
                return;
            }
        }
    }

    private bool TargetIsInFront(GameObject target)
    {
        GraphicRaycaster gr = this.GetComponent<GraphicRaycaster>();
        PointerEventData ped = new PointerEventData(null);
        ped.position = GetComponent<Canvas>().worldCamera.WorldToScreenPoint(cursor.position);
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(ped, results);
        if (Object.ReferenceEquals(results[0].gameObject, target))
        {
            return true;
        }
        return false;
    }

    private bool PointInsideRect(Vector2 point, RectTransform rectTransform)
    {
        Rect rect = rectTransform.rect;
        Vector2 rectMin = (Vector2)rectTransform.position - rectTransform.pivot * (Vector2)rectTransform.transform.TransformVector(rect.size);
        Vector2 rectMax = rectMin + (Vector2)rectTransform.transform.TransformVector(rect.size);

        return point.x >= rectMin.x && point.x <= rectMax.x &&
               point.y >= rectMin.y && point.y <= rectMax.y;
    }
}

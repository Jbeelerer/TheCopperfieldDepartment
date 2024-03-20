using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
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
    public RectTransform background;
    public RectTransform taskBar;
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
            CheckWindowMouseDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Clicked window
            CheckWindowMouseUp();

            // Clicked an app
            CheckAppMouseUp();

            OSWindow windowToBeDeleted = null;
            foreach (OSWindow window in windows)
            {
                // Close window if dragged below taskbar
                if (window.isMoving && PointInsideRect(cursor.position, taskBar))
                {
                    windowToBeDeleted = window;
                }

                window.isMoving = false;
            }

            if (windowToBeDeleted)
            {
                windows.Remove(windowToBeDeleted);
                Destroy(windowToBeDeleted.gameObject);
            }
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

    private void CheckWindowMouseDown()
    {
        foreach (OSWindow window in windows)
        {
            if (PointInsideRect(cursor.position, window.topBar))
            {
                // Check if window is behind another element
                if (TargetIsInFront(window.topBar.gameObject))
                {
                    // Start moving the affected window with the cursor
                    window.isMoving = true;
                    window.transform.SetAsLastSibling();

                    // Reset window to small mode if moved while in another size mode
                    if (!window.isInSmallMode) {
                        ResizeWindowSmall(window);
                        window.rectTrans.position = new Vector2(cursor.position.x, cursor.position.y - window.rectTrans.anchorMax.y);
                    }

                    return;
                }
            }
        }
    }

    private void CheckWindowMouseUp()
    {
        foreach (OSWindow window in windows)
        {
            // Clicked buttonClose
            if (PointInsideRect(cursor.position, window.buttonClose))
            {
                // Check if window is behind another element
                if (TargetIsInFront(window.buttonClose.gameObject))
                {
                    windows.Remove(window);
                    Destroy(window.gameObject);
                    return;
                }
            }
            // Clicked buttonSmall
            else if (PointInsideRect(cursor.position, window.buttonSmall))
            {
                // Check if window is behind another element
                if (TargetIsInFront(window.buttonSmall.gameObject))
                {
                    // Make window small
                    ResizeWindowSmall(window);
                    return;
                }
            }
            // Clicked buttonLong
            else if (PointInsideRect(cursor.position, window.buttonLong))
            {
                // Check if window is behind another element
                if (TargetIsInFront(window.buttonLong.gameObject))
                {
                    // Make window long, position on right
                    window.rectTrans.anchorMin = new Vector2(0.5f, 0);// new Vector2(screen.pivot.x, screen.anchorMin.y);
                    window.rectTrans.anchorMax = new Vector2(1, 1);// screen.anchorMax;
                    window.rectTrans.offsetMin = new Vector2(0, 0);
                    window.rectTrans.offsetMax = new Vector2(0, 0);
                    window.isInSmallMode = false;
                    return;
                }
            }
            // Clicked buttonBig
            else if (PointInsideRect(cursor.position, window.buttonBig))
            {
                // Check if window is behind another element
                if (TargetIsInFront(window.buttonBig.gameObject))
                {
                    // Make window fullscreen
                    window.rectTrans.anchorMin = new Vector2(0, 0);
                    window.rectTrans.anchorMax = new Vector2(1, 1);
                    window.rectTrans.offsetMin = new Vector2(0, 0);
                    window.rectTrans.offsetMax = new Vector2(0, 0);
                    window.isInSmallMode = false;
                    return;
                }
            }
        }
    }

    private void ResizeWindowSmall(OSWindow window)
    {
        window.rectTrans.anchorMin = new Vector2(0.5f, 0.5f);
        window.rectTrans.anchorMax = new Vector2(0.5f, 0.5f);
        window.rectTrans.sizeDelta = new Vector2(450, 300);
        window.isInSmallMode = true;
    }

    private void CheckAppMouseUp()
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
                GameObject newWindow = Instantiate(windowPrefab, transform.position, transform.rotation, background.transform);
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

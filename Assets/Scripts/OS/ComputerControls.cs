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
    public float mouseSensitivity = 1;
    public RectTransform cursor;
    public RectTransform background;
    public RectTransform taskBar;
    public Transform tabContainer;
    public OSWindow testWindow;
    public OSTab testTab;
    public GameObject windowPrefab;
    public GameObject tabPrefab;
    public List<OSApplication> apps = new List<OSApplication>();

    private RectTransform screen;
    private float mouseSpeedX;
    private float mouseSpeedY;
    private List<OSWindow> windows = new List<OSWindow>();
    private OSWindow rightWindow;
    private OSWindow leftWindow;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = false;

        screen = GetComponent<RectTransform>();
        windows.Add(testWindow);
        testWindow.associatedTab = testTab;
    }

    // Update is called once per frame
    void Update()
    {
        mouseSpeedX = mouseSensitivity * Input.GetAxis("Mouse X");
        mouseSpeedY = mouseSensitivity * Input.GetAxis("Mouse Y");

        if (Input.GetMouseButtonDown(0))
        {
            // Clicked window
            CheckWindowMouseDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Clicked window
            CheckWindowMouseUp();

            // Clicked an app
            CheckAppMouseUp();

            // Clicked a tab
            CheckTabMouseUp();

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
                Destroy(windowToBeDeleted.associatedTab.gameObject);
                Destroy(windowToBeDeleted.gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        // Move cursor
        cursor.anchoredPosition += new Vector2(mouseSpeedX, mouseSpeedY);

        // Move selected window with the cursor
        foreach (OSWindow window in windows)
        {
            if (window.isMoving)
            {
                window.MoveWindow(new Vector2(mouseSpeedX, mouseSpeedY));
            }
        }

        // Reverse the movement if the cursor went outside the screen
        if (!PointInsideRect(cursor.position, screen))
        {
            cursor.anchoredPosition -= new Vector2(mouseSpeedX, mouseSpeedY);

            foreach (OSWindow window in windows)
            {
                if (window.isMoving)
                {
                    window.MoveWindow(-(new Vector2(mouseSpeedX, mouseSpeedY)));
                }
            }
        }
    }

    private void CheckAppMouseUp()
    {
        foreach (OSApplication app in apps)
        {
            GameObject hitObject = GetFirstHitObject();
            if (Object.ReferenceEquals(hitObject, app.gameObject))
            {
                // Check if the window is already open
                foreach (OSWindow window in windows)
                {
                    if (window.appType == app.appType)
                    {
                        return;
                    }
                }
                // Create window
                GameObject newWindow = Instantiate(windowPrefab, transform.position, transform.rotation, background.transform);
                newWindow.GetComponent<OSWindow>().appType = app.appType;
                windows.Add(newWindow.GetComponent<OSWindow>());
                // Create tab
                GameObject newTab = Instantiate(tabPrefab, transform.position, transform.rotation, tabContainer.transform);
                newTab.GetComponent<OSTab>().appType = app.appType;
                newWindow.GetComponent<OSWindow>().associatedTab = newTab.GetComponent<OSTab>();
                return;
            }
        }
    }

    private void CheckTabMouseUp()
    {
        foreach (OSWindow window in windows)
        {
            // Check if a tab was hit
            GameObject hitObject = GetFirstHitObject();
            if (Object.ReferenceEquals(hitObject, window.associatedTab.gameObject))
            {
                BringWindowToFront(window);
            }
        }
    }

    private void CheckWindowMouseDown()
    {
        foreach (OSWindow window in windows)
        {
            if (PointInsideRect(cursor.position, window.rectTrans))
            {
                GameObject hitObject = GetFirstHitObject();

                // Check if any parent of the hit object is the window
                GameObject nextParentObj = hitObject;
                while (nextParentObj.transform.parent != null)
                {
                    nextParentObj = nextParentObj.transform.parent.gameObject;
                    if (Object.ReferenceEquals(nextParentObj, window.gameObject))
                    {
                        BringWindowToFront(window);
                        break;
                    }
                }

                // Check if topbar was hit
                if (Object.ReferenceEquals(hitObject, window.topBar.gameObject))
                {
                    // Start moving the affected window with the cursor
                    window.isMoving = true;

                    // Reset window to small mode if moved while in another size mode
                    if (window.currWindowSize != WindowSize.SMALL)
                    {
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
            if (PointInsideRect(cursor.position, window.rectTrans))
            {
                // Check which element was hit
                GameObject hitObject = GetFirstHitObject();

                if (Object.ReferenceEquals(hitObject, window.buttonClose.gameObject))
                {
                    // Close window
                    RemoveLeftRightWindow(window);
                    windows.Remove(window);
                    Destroy(window.associatedTab.gameObject);
                    Destroy(window.gameObject);
                    return;
                }
                else if (Object.ReferenceEquals(hitObject, window.buttonSmall.gameObject))
                {
                    // Make window small
                    ResizeWindowSmall(window);
                    return;
                }
                else if (Object.ReferenceEquals(hitObject, window.buttonLong.gameObject))
                {
                    // Make window long, position on left or right
                    if (!leftWindow)
                    {
                        ResizeWindowLongLeft(window);
                    }
                    else if (window != leftWindow)
                    {
                        ResizeWindowLongRight(window);
                    }
                    return;
                }
                else if (Object.ReferenceEquals(hitObject, window.buttonBig.gameObject))
                {
                    // Make window fullscreen
                    ResizeWindowBig(window);
                    return;
                }
                else if (Object.ReferenceEquals(hitObject, window.sideswapRight.gameObject))
                {
                    // Swap current left window with the one on the right
                    if (window.currWindowSize == WindowSize.LONG_LEFT)
                    {
                        RemoveLeftRightWindow(window);
                        if (rightWindow)
                        {
                            ResizeWindowLongLeft(rightWindow);
                        }
                        ResizeWindowLongRight(window);
                    }
                    return;
                }
                else if (Object.ReferenceEquals(hitObject, window.sideswapLeft.gameObject))
                {
                    // Swap current right window with the one on the left
                    if (window.currWindowSize == WindowSize.LONG_RIGHT)
                    {
                        RemoveLeftRightWindow(window);
                        if (leftWindow)
                        {
                            ResizeWindowLongRight(leftWindow);
                        }
                        ResizeWindowLongLeft(window);
                    }
                    return;
                }
            }
        }
    }

    private void BringWindowToFront(OSWindow window)
    {
        // Bring clicked window to front
        window.transform.SetAsLastSibling();

        // Set as current right/left window if in long mode
        if (window.currWindowSize == WindowSize.LONG_LEFT)
        {
            leftWindow = window;
        }
        else if (window.currWindowSize == WindowSize.LONG_RIGHT)
        {
            rightWindow = window;
        }
    }

    private void RemoveLeftRightWindow(OSWindow window)
    {
        if (Object.ReferenceEquals(window, leftWindow))
        {
            leftWindow = null;
        }
        else if (Object.ReferenceEquals(window, rightWindow))
        {
            rightWindow = null;
        }
    }

    private void ResizeWindowSmall(OSWindow window)
    {
        RemoveLeftRightWindow(window);
        window.rectTrans.anchorMin = new Vector2(0.5f, 0.5f);
        window.rectTrans.anchorMax = new Vector2(0.5f, 0.5f);
        window.rectTrans.sizeDelta = new Vector2(450, 300);
        window.currWindowSize = WindowSize.SMALL;
    }

    private void ResizeWindowLongLeft(OSWindow window)
    {
        window.rectTrans.anchorMin = new Vector2(0, 0);
        window.rectTrans.anchorMax = new Vector2(0.5f, 1);
        window.rectTrans.offsetMin = new Vector2(0, 0);
        window.rectTrans.offsetMax = new Vector2(0, 0);
        leftWindow = window;
        window.currWindowSize = WindowSize.LONG_LEFT;
    }

    private void ResizeWindowLongRight(OSWindow window)
    {
        window.rectTrans.anchorMin = new Vector2(0.5f, 0);
        window.rectTrans.anchorMax = new Vector2(1, 1);
        window.rectTrans.offsetMin = new Vector2(0, 0);
        window.rectTrans.offsetMax = new Vector2(0, 0);
        rightWindow = window;
        window.currWindowSize = WindowSize.LONG_RIGHT;
    }

    private void ResizeWindowBig(OSWindow window)
    {
        RemoveLeftRightWindow(window);
        window.rectTrans.anchorMin = new Vector2(0, 0);
        window.rectTrans.anchorMax = new Vector2(1, 1);
        window.rectTrans.offsetMin = new Vector2(0, 0);
        window.rectTrans.offsetMax = new Vector2(0, 0);
        window.currWindowSize = WindowSize.BIG;
    }

    // Shoot raycast at current cursor position and get the first (topmost) hit object
    private GameObject GetFirstHitObject()
    {
        GraphicRaycaster gr = this.GetComponent<GraphicRaycaster>();
        PointerEventData ped = new PointerEventData(null);
        ped.position = GetComponent<Canvas>().worldCamera.WorldToScreenPoint(cursor.position);
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(ped, results);
        return results[0].gameObject;
    }

    // Check if point is inside a given rect
    private bool PointInsideRect(Vector2 point, RectTransform rectTransform)
    {
        Rect rect = rectTransform.rect;
        Vector2 rectMin = (Vector2)rectTransform.position - rectTransform.pivot * (Vector2)rectTransform.transform.TransformVector(rect.size);
        Vector2 rectMax = rectMin + (Vector2)rectTransform.transform.TransformVector(rect.size);

        return point.x >= rectMin.x && point.x <= rectMax.x &&
               point.y >= rectMin.y && point.y <= rectMax.y;
    }
}

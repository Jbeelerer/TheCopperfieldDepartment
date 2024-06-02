using SaveSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum OSAppType
{
    TEST,
    SOCIAL,
    GOV,
    SETTINGS,
    PEOPLE_LIST,
    WARNING,
    START_SETTINGS
}

public enum OSInvestigationState
{
    NONE,
    PERSON_ACCUSED,
    POST_DELETED
}

public class ComputerControls : MonoBehaviour, ISavable
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
    public Sprite cursorNormal;
    public Sprite cursorClickable;
    public Sprite cursorForbidden;
    public OSInvestigationState investigationState = OSInvestigationState.NONE;
    public TextMeshProUGUI computerTime;
    public TextMeshProUGUI computerDate;
    public OSPointySystem pointySystem;
    public Sprite[] appIcons;

    private RectTransform screen;
    private float mouseSpeedX;
    private float mouseSpeedY;
    private List<OSWindow> windows = new List<OSWindow>();
    private OSWindow rightWindow;
    private OSWindow leftWindow;
    private GameObject cursorTooltip;
    private bool cursorStopped = false;
    private float timeCursorStopped = 0;
    private float tooltipDelay = 0.3f;
    private GameManager gm;
    private bool cursorActive = false;
    private OSWindow currentFocusedWindow;

    [SerializeField] private RectTransform halfScreenBlockadeLeft;
    [SerializeField] private RectTransform halfScreenBlockadeRight;
    [SerializeField] private AudioClip windowOpenSound;
    [SerializeField] private AudioClip clickDownSound;
    [SerializeField] private AudioClip clickUpSound;

    [HideInInspector] public AudioManager audioManager;

    public PinEvent OnUnpinned;

    // Start is called before the first frame update
    void Start()
    {
        //UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        UnityEngine.Cursor.visible = false;
        cursor.gameObject.SetActive(cursorActive);

        screen = GetComponent<RectTransform>();
        //windows.Add(testWindow);
        //testWindow.associatedTab = testTab;
        cursorTooltip = cursor.transform.Find("Tooltip").gameObject;

        cursor.anchoredPosition = new Vector2(-1000, -1000);

        gm = GameManager.instance;
        audioManager = AudioManager.instance;

        gm.OnNewDay.AddListener(CloseAllWindows);

        // Run stuff on first day only
        if (gm.GetDay() == 1)
        {
            OpenWindow(OSAppType.START_SETTINGS);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!cursorActive)
            return;

        mouseSpeedX = mouseSensitivity * Input.GetAxis("Mouse X");
        mouseSpeedY = mouseSensitivity * Input.GetAxis("Mouse Y");

        // Move cursor
        MoveMouse();

        if (Input.GetMouseButtonDown(0))
        {
            // Play click down sound
            audioManager.PlayAudio(clickDownSound);

            // Clicked window
            CheckWindowMouseDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Play click up sound
            audioManager.PlayAudio(clickUpSound);

            // Check if pointy is active and should progress
            CheckPointyProgress();

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

                // Put window into halfscreen mode if dragged to border of screen
                if (window.isMoving)
                {
                    if (PointInsideRect(cursor.position, halfScreenBlockadeLeft))
                    {
                        ResizeWindowLongLeft(window);
                    }
                    else if (PointInsideRect(cursor.position, halfScreenBlockadeRight))
                    {
                        ResizeWindowLongRight(window);
                    }
                }

                window.isMoving = false;
            }

            if (windowToBeDeleted)
            {
                CloseWindow(windowToBeDeleted);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!cursorActive)
            return;

        // Update computer time
        computerTime.text = System.DateTime.Now.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US"));

        // Check if cursor is still and tooltip should be displayed
        CheckForTooltip();

        GameObject hitObject = GetFirstHitObject();
        // Change cursor sprite if hovering over button
        if (hitObject && (!hitObject.GetComponent<TextMeshProUGUI>() || hitObject.GetComponent<Button>()))
        {
            cursor.GetComponent<Image>().sprite = hitObject.GetComponent<Button>() ? cursorClickable : cursorNormal;
        }
        // Change cursor sprite if hovering over disabled button
        if (hitObject && hitObject.GetComponent<Button>() && !hitObject.GetComponent<Button>().isActiveAndEnabled)
        {
            cursor.GetComponent<Image>().sprite = cursorForbidden;
        }
    }

    private void MoveMouse()
    {
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

    public void CheckPointyProgress()
    {
        // If Pointy is active, he will progress if the next target object is clicked
        if (GetFirstHitObject() != pointySystem.spotlight && GetFirstHitObject() != pointySystem.pointyButton
            && pointySystem.GetNextTargetObject() && PointInsideRect(cursor.position, pointySystem.GetNextTargetObject().GetComponent<RectTransform>()))
        {
            pointySystem.ProgressPointy();
        }
    }

    public void TogglePointy(bool toggledAutomatically = false)
    {
        // Close pointy if currently active
        if (pointySystem.GetNextTargetObject())
        {
            pointySystem.HidePointy();
            return;
        }

        if (!currentFocusedWindow)
        {
            // Start desktop tutorial because no specific window is focused
            pointySystem.StartTutorial("Desktop", toggledAutomatically);
            return;
        }

        ResizeWindowSmall(currentFocusedWindow);
        currentFocusedWindow.rectTrans.position = screen.position;

        switch (currentFocusedWindow.appType)
        {
            case OSAppType.SOCIAL:
                pointySystem.StartTutorial("SocialMedia", toggledAutomatically);
                break;
            case OSAppType.GOV:
                pointySystem.StartTutorial("GovApp", toggledAutomatically);
                break;
            case OSAppType.PEOPLE_LIST:
                pointySystem.StartTutorial("PeopleList", toggledAutomatically);
                break;
            case OSAppType.START_SETTINGS:
                pointySystem.StartTutorial("StartSettings", toggledAutomatically);
                break;
            default:
                pointySystem.StartTutorial("Default", toggledAutomatically);
                break;
        }
    }

    public SocialMediaPost[] GetPosts()
    {
        return gm.GetPosts();
    }

    public Person[] GetPeople()
    {
        return gm.GetPeople();
    }

    public Mail[] GetMails()
    {
        return gm.GetMails();
    }

    public void LeaveComputer()
    {
        gm.SetGameState(GameState.Playing);
        ToggleCursor();
    }

    public void ToggleCursor()
    {
        Invoke("ToggleCursorDelayed", 0.25f);
    }

    private void ToggleCursorDelayed()
    {
        cursorActive = !cursorActive;
        cursor.gameObject.SetActive(cursorActive);
        cursor.anchoredPosition = cursorActive ? new Vector2(0, 0) : new Vector2(-1000, -1000);
    }

    private void CheckForTooltip()
    {
        // Start tooltip delay timer if cursor stopped
        if (!cursorStopped && mouseSpeedX == 0 && mouseSpeedY == 0)
        {
            cursorStopped = true;
            timeCursorStopped = Time.fixedTime;
        }
        else if (Mathf.Abs(mouseSpeedX) != 0 || Mathf.Abs(mouseSpeedY) != 0)
        {
            cursorStopped = false;
            // If cursor moving, remove potential tooltip
            if (cursorTooltip.activeInHierarchy)
            {
                cursorTooltip.SetActive(false);
            }
        }
        // Display tooltip if tooltip delay passed and object is tooltippable
        if (!cursorTooltip.activeInHierarchy && Time.fixedTime > timeCursorStopped + tooltipDelay)
        {
            GameObject hitObject = GetFirstHitObject();
            if (!hitObject)
                return;

            if (hitObject.GetComponent<Tooltippable>())
            {
                cursorTooltip.GetComponentInChildren<TextMeshProUGUI>().text = GetFirstHitObject().GetComponent<Tooltippable>().tooltipText;
                cursorTooltip.SetActive(true);
            }
        }
    }

    private void CheckAppMouseUp()
    {
        foreach (OSApplication app in apps)
        {
            GameObject hitObject = GetFirstHitObject();
            if (!hitObject)
                return;

            if (Object.ReferenceEquals(hitObject, app.gameObject))
            {
                // Remove Notif icon if present
                if (app.transform.Find("Notif"))
                {
                    app.transform.Find("Notif").gameObject.SetActive(false);
                }

                OpenWindow(app.appType);
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
            if (!hitObject)
                return;

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
                if (!hitObject)
                    return;

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
                    if (window.canBeMoved)
                    {
                        window.isMoving = true;
                    }

                    // Reset window to small mode if moved while in another size mode
                    if (window.currWindowSize != WindowSize.SMALL)
                    {
                        ResizeWindowSmall(window);
                        window.rectTrans.position = new Vector2(cursor.position.x, cursor.position.y - window.rectTrans.anchorMax.y);
                    }

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

    private void CheckWindowMouseUp()
    {
        foreach (OSWindow window in windows)
        {
            if (window.rectTrans && PointInsideRect(cursor.position, window.rectTrans))
            {
                // Check which element was hit
                GameObject hitObject = GetFirstHitObject();

                if (!hitObject)
                {
                    return;
                }
                else if (window.buttonClose && Object.ReferenceEquals(hitObject, window.buttonClose.gameObject))
                {
                    // Close window
                    CloseWindow(window);
                    return;
                }
                else if (window.buttonSmall && Object.ReferenceEquals(hitObject, window.buttonSmall.gameObject))
                {
                    // Make window small
                    ResizeWindowSmall(window);
                    return;
                }
                else if (window.buttonLong && Object.ReferenceEquals(hitObject, window.buttonLong.gameObject))
                {
                    // Make window long, position on left or right
                    if (!rightWindow)
                    {
                        ResizeWindowLongRight(window);
                    }
                    else if (window != rightWindow)
                    {
                        ResizeWindowLongLeft(window);
                    }
                    return;
                }
                else if (window.buttonBig && Object.ReferenceEquals(hitObject, window.buttonBig.gameObject))
                {
                    // Make window fullscreen
                    ResizeWindowBig(window);
                    return;
                }
            }
        }
    }

    private void HideWindowSizeButton(OSWindow window, RectTransform button)
    {
        // Dont show buttonBig for the moment
        //window.buttonBig.gameObject.SetActive(true);
        window.buttonLong.gameObject.SetActive(true);
        window.buttonSmall.gameObject.SetActive(true);
        button.gameObject.SetActive(false);
    }

    private void CloseWindow(OSWindow window)
    {
        ResizeWindowSmall(window);
        currentFocusedWindow = null;
        //pointySystem.ToggleButtonNotif("Default");
        window.associatedTab.gameObject.SetActive(false);
        window.gameObject.SetActive(false);
    }

    private void CloseAllWindows()
    {
        foreach (OSWindow window in windows)
        {
            CloseWindow(window);
            Destroy(window.associatedTab.gameObject);
            Destroy(window.gameObject);
        }
        windows.Clear();
    }

    public void OpenWindow(OSAppType type, string warningMessage = "Warning message", System.Action successFunc = null)
    {
        // PLay opening sound
        audioManager.PlayAudio(windowOpenSound);
        // Check if the window is already open
        foreach (OSWindow window in windows)
        {
            if (window.appType == type)
            {
                // Reveal window and set back to screen middle if it exists but isn't active
                if (!window.gameObject.activeInHierarchy)
                {
                    window.gameObject.SetActive(true);
                    SetWindowOpenPosition(window);
                    window.associatedTab.gameObject.SetActive(true);
                }
                BringWindowToFront(window);
                return;
            }
        }
        // Create window
        GameObject newWindow = Instantiate(windowPrefab, transform.position, transform.rotation, background.transform);
        newWindow.GetComponent<OSWindow>().appType = type;
        newWindow.GetComponent<OSWindow>().warningMessage = warningMessage;
        newWindow.GetComponent<OSWindow>().warningSuccessFunc = successFunc;
        BringWindowToFront(newWindow.GetComponent<OSWindow>());
        // Don't add to open windows list if its a temporary window like a warning
        if (newWindow.GetComponent<OSWindow>().appType != OSAppType.WARNING && newWindow.GetComponent<OSWindow>().appType != OSAppType.START_SETTINGS)
            windows.Add(newWindow.GetComponent<OSWindow>());
        // Make warning window smaller than normal windows
        if (newWindow.GetComponent<OSWindow>().appType == OSAppType.WARNING)
            newWindow.GetComponent<OSWindow>().rectTrans.sizeDelta = new Vector2(300, 200);
        // Create tab
        GameObject newTab = Instantiate(tabPrefab, transform.position, transform.rotation, tabContainer.transform);
        newTab.GetComponent<OSTab>().appType = type;
        newWindow.GetComponent<OSWindow>().associatedTab = newTab.GetComponent<OSTab>();
        // Set window position
        SetWindowOpenPosition(newWindow.GetComponent<OSWindow>());
        // Open Pointy Tutorial if it exists for the window
        if (/*gm.GetDay() == 1 && */newWindow.GetComponent<OSWindow>().appType != OSAppType.WARNING && newWindow.GetComponent<OSWindow>().appType != OSAppType.START_SETTINGS)
            TogglePointy(true);
    }

    private void SetWindowOpenPosition(OSWindow window)
    {
        if (window.appType == OSAppType.WARNING)
        {
            window.rectTrans.position = screen.position;
            return;
        }

        // If a window is already positioned perfectly in the middle, move the new one slightly left and down, repeat until a free spot is found
        Vector3 currentPos = screen.position;
        bool windowPlaced = false;
        while (!windowPlaced)
        {
            bool progressCurrentPos = false;
            foreach (OSWindow w in windows)
            {
                if (w.gameObject.activeInHierarchy && Mathf.Approximately(w.rectTrans.position.x, currentPos.x) && Mathf.Approximately(w.rectTrans.position.y, currentPos.y))
                {
                    currentPos += new Vector3(0.1f, -0.1f);
                    progressCurrentPos = true;
                    break;
                }
            }
            if (!progressCurrentPos)
            {
                window.GetComponent<OSWindow>().rectTrans.position = currentPos;
                windowPlaced = true;
            }
        }
    }

    private void BringWindowToFront(OSWindow window)
    {
        // Bring clicked window to front
        window.transform.SetAsLastSibling();
        currentFocusedWindow = window;
        foreach (OSWindow w in windows)
        {
            w.topBar.GetComponent<Image>().color = new Color(w.topBar.GetComponent<Image>().color.r, w.topBar.GetComponent<Image>().color.g, w.topBar.GetComponent<Image>().color.b, 0.6f);
            if (w.associatedTab)
                w.associatedTab.GetComponent<Image>().sprite = w.associatedTab.unclickedImage;
        }
        window.topBar.GetComponent<Image>().color = new Color(window.topBar.GetComponent<Image>().color.r, window.topBar.GetComponent<Image>().color.g, window.topBar.GetComponent<Image>().color.b, 1f);
        if (window.associatedTab)
            window.associatedTab.GetComponent<Image>().sprite = window.associatedTab.clickedImage;

        // Set as current right/left window if in long mode
        if (window.currWindowSize == WindowSize.LONG_LEFT)
        {
            leftWindow = window;
        }
        else if (window.currWindowSize == WindowSize.LONG_RIGHT)
        {
            rightWindow = window;
        }

        // Activate pointy notif if window tutorial hasnt been seen yet
        /*if (gm.GetDay() == 1)
        {
            switch (currentFocusedWindow.appType)
            {
                case OSAppType.SOCIAL:
                    pointySystem.ToggleButtonNotif("SocialMedia");
                    break;
                case OSAppType.GOV:
                    pointySystem.ToggleButtonNotif("GovApp");
                    break;
                case OSAppType.PEOPLE_LIST:
                    pointySystem.ToggleButtonNotif("PeopleList");
                    break;
                default:
                    pointySystem.ToggleButtonNotif("Default");
                    break;
            }
        }*/
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
        window.rectTrans.sizeDelta = new Vector2(400, 300);
        window.currWindowSize = WindowSize.SMALL;
        HideWindowSizeButton(window, window.buttonSmall);
    }

    private void ResizeWindowLongLeft(OSWindow window)
    {
        window.rectTrans.anchorMin = new Vector2(0, 0);
        window.rectTrans.anchorMax = new Vector2(0.5f, 1);
        window.rectTrans.offsetMin = new Vector2(0, 0);
        window.rectTrans.offsetMax = new Vector2(0, 0);
        leftWindow = window;
        window.currWindowSize = WindowSize.LONG_LEFT;
        HideWindowSizeButton(window, window.buttonLong);
    }

    private void ResizeWindowLongRight(OSWindow window)
    {
        window.rectTrans.anchorMin = new Vector2(0.5f, 0);
        window.rectTrans.anchorMax = new Vector2(1, 1);
        window.rectTrans.offsetMin = new Vector2(0, 0);
        window.rectTrans.offsetMax = new Vector2(0, 0);
        rightWindow = window;
        window.currWindowSize = WindowSize.LONG_RIGHT;
        HideWindowSizeButton(window, window.buttonLong);
    }

    private void ResizeWindowBig(OSWindow window)
    {
        RemoveLeftRightWindow(window);
        window.rectTrans.anchorMin = new Vector2(0, 0);
        window.rectTrans.anchorMax = new Vector2(1, 1);
        window.rectTrans.offsetMin = new Vector2(0, 0);
        window.rectTrans.offsetMax = new Vector2(0, 0);
        window.currWindowSize = WindowSize.BIG;
        HideWindowSizeButton(window, window.buttonBig);
    }

    // Shoot raycast at current cursor position and get the first (topmost) hit object
    public GameObject GetFirstHitObject()
    {
        GraphicRaycaster gr = this.GetComponent<GraphicRaycaster>();
        PointerEventData ped = new PointerEventData(null);
        ped.position = GetComponent<Canvas>().worldCamera.WorldToScreenPoint(cursor.position);
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(ped, results);
        if (results.Count <= 0)
        {
            return null;
        }
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

    public void LoadData(SaveData data)
    {
        mouseSensitivity = data.mouseSensitivity;
    }

    public void SaveData(SaveData data)
    {
        data.mouseSensitivity = mouseSensitivity;
    }
}

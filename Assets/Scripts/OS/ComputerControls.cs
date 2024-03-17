using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class ComputerControls : MonoBehaviour
{
    public float speed = 1;
    public RectTransform cursor;
    public OSWindow testWindow;

    private RectTransform screen;
    private float h;
    private float v;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        screen = GetComponent<RectTransform>();
        cursor.anchoredPosition += new Vector2(0, 15 * speed);
    }

    // Update is called once per frame
    void Update()
    {
        /*Vector3 point = new Vector3();
        Vector2 mousePos = new Vector2();
        Vector2 newPos = new Vector2();
        mousePos = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(this.GetComponent<RectTransform>(), mousePos, cam, out newPos);
        //point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));
        cursor.anchoredPosition = newPos;*/

        h = speed * Input.GetAxis("Mouse X");
        v = speed * Input.GetAxis("Mouse Y");

        if (Input.GetMouseButtonDown(0))
        {
            //print(cursor.anchoredPosition);
            if (RectsOverlapping(cursor, testWindow.topBar))
            {
                testWindow.isMoving = true;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            testWindow.isMoving = false;
        }
    }

    private void FixedUpdate()
    {
        cursor.anchoredPosition += new Vector2(h, v);

        if (testWindow.isMoving)
        {
            testWindow.MoveWindow(new Vector2(h, v));
        }

        // Reverse the movement if the cursor went outside the screen
        if (!RectsOverlapping(cursor, screen))
        {
            cursor.anchoredPosition -= new Vector2(h, v);

            if (testWindow.isMoving)
            {
                testWindow.MoveWindow(-(new Vector2(h, v)));
            }
        }
    }

    private bool RectsOverlapping(RectTransform firstRect, RectTransform secondRect)
    {
        if (firstRect.position.x + firstRect.transform.TransformVector(firstRect.rect.size).x * 0.5f < secondRect.position.x - secondRect.transform.TransformVector(secondRect.rect.size).x * 0.5f)
        {
            return false;
        }
        if (secondRect.position.x + secondRect.transform.TransformVector(secondRect.rect.size).x * 0.5f < firstRect.position.x - firstRect.transform.TransformVector(firstRect.rect.size).x * 0.5f)
        {
            return false;
        }
        if (firstRect.position.y + firstRect.transform.TransformVector(firstRect.rect.size).y * 0.5f < secondRect.position.y - secondRect.transform.TransformVector(secondRect.rect.size).y * 0.5f)
        {
            return false;
        }
        if (secondRect.position.y + secondRect.transform.TransformVector(secondRect.rect.size).y * 0.5f < firstRect.position.y - firstRect.transform.TransformVector(firstRect.rect.size).y * 0.5f)
        {
            return false;
        }
        return true;
    }
}

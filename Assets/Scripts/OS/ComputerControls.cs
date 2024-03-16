using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerControls : MonoBehaviour
{
    public RectTransform cursor;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 point = new Vector3();
        Event currentEvent = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = currentEvent.mousePosition.x;
        mousePos.y = this.GetComponent<RectTransform>().rect.height - currentEvent.mousePosition.y;

        cursor.position = mousePos;

    }
}

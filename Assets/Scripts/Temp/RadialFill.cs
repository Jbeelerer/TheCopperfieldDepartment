using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RadialFill : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public void OnDrag(PointerEventData ped)
    {
        Vector2 pos = default(Vector2);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), ped.position, ped.pressEventCamera, out pos))
        {
            pos.x = (pos.x / GetComponent<RectTransform>().sizeDelta.x);
            pos.y = (pos.y / GetComponent<RectTransform>().sizeDelta.y);

            float angle = Mathf.Atan2(pos.y, pos.x);
            float degrees = (angle * Mathf.Rad2Deg) + 180;

            transform.rotation = Quaternion.Euler(0, 0, degrees);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

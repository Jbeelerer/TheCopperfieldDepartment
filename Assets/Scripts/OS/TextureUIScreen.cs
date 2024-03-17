using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextureUIScreen : GraphicRaycaster
{
    public Camera sc; // the camera that renders the UI

    public RectTransform cursor;

    // HAS to be a mesh collider, with proper texture coordinates.
    // in my game it's a curved crt monitor, but the UVs have to be a square
    public Collider targetCollider;

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //print(hit.transform.gameObject.name);
            if (hit.collider == targetCollider)
            {
                /*Vector3 pos = new(hit.textureCoord.x, hit.textureCoord.y);
                pos.x = pos.x * this.GetComponent<RectTransform>().rect.width - this.GetComponent<RectTransform>().rect.width / 2;
                pos.y = pos.y * this.GetComponent<RectTransform>().rect.height - this.GetComponent<RectTransform>().rect.height / 2;
                cursor.anchoredPosition = pos;*/

                //print(hit.transform.gameObject.name + " " + pos);
                Vector3 pos = new(hit.textureCoord.x, hit.textureCoord.y);
                pos.x *= sc.targetTexture.width;
                pos.y *= sc.targetTexture.height;

                eventData.position = pos;

                base.Raycast(eventData, resultAppendList);
            }
        }
    }

}

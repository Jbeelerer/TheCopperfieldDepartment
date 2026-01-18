using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PhoneHighlight : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image image;
    private GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        gm = FindObjectOfType<GameManager>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StopHighlight()
    {
        image.enabled = false;
        StopAllCoroutines();
    }

   public IEnumerator PhoneRinging(Transform phonePosition)
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

          /*  if (gm.GetGameState() != GameState.Playing)
            {
                rectTransform.localScale = new Vector3(0, 0, 0);
                while (gm.GetGameState() != GameState.Playing)
                {
                    yield return new WaitForEndOfFrame();   
                }
            }*/

            Vector3 phoneWorldPos = phonePosition.position;
            Vector3 screenPos = Camera.main.WorldToViewportPoint(phoneWorldPos);

            if (screenPos.z > 0 &&
                screenPos.x > 0 && screenPos.x < 1 &&
                screenPos.y > 0 && screenPos.y < 1)
            {
                // Phone is visible on screen -> hide indicator
                image.enabled = false;
            }
            else
            {
                image.enabled = true;

                // Direction from camera to phone in world space
                Vector3 dir = (phoneWorldPos - Camera.main.transform.position).normalized;

                // Project onto screen plane
                Vector3 forward = Camera.main.transform.forward;
                Vector3 right = Camera.main.transform.right;
                Vector3 up = Camera.main.transform.up;

                // Angle in screen space
                float x = Vector3.Dot(dir, right);
                float y = Vector3.Dot(dir, up);

                Vector2 arrowDir = new Vector2(x, y).normalized;

                // Position on screen edge
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                float edge = Mathf.Min(Screen.width, Screen.height) * 1f; // distance from center
                Vector2 targetPos = screenCenter + arrowDir * edge;

                rectTransform.position = targetPos;

                // Rotate arrow to point towards phone
                float angle = Mathf.Atan2(arrowDir.y, arrowDir.x) * Mathf.Rad2Deg;
                rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90);

                // Pulsate scale
                float pulse = Mathf.Lerp(5f, 15f, Mathf.PingPong(Time.time, 1f));
                rectTransform.localScale = Vector3.one * pulse / Vector3.Distance(phoneWorldPos, Camera.main.transform.position);
            }
        }
    }

}

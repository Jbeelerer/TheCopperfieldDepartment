using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DestroyAfterTime : MonoBehaviour
{
    public float seconds = 1.0f;
    public bool isDay = true;

    [SerializeField] private Image image;
    [SerializeField] private bool fadeAway = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyAfterSeconds(seconds));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator DestroyAfterSeconds(float seconds)
    {
        if (fadeAway)
        {
            yield return new WaitForSeconds(seconds - 0.5f);

            // Ensure alpha is initialized
            image.transform.GetChild(0).GetComponent<TMP_Text>().canvasRenderer.SetAlpha(1f);
            image.transform.GetChild(1).GetComponent<TMP_Text>().canvasRenderer.SetAlpha(1f);

            // Fade out over 2 seconds  
            image.CrossFadeAlpha(0f, 0.5f, false);  
            StartCoroutine(FadeText(image.transform.GetChild(0).GetComponent<TMP_Text>(), 0.5f));
            StartCoroutine(FadeText(image.transform.GetChild(1).GetComponent<TMP_Text>(), 0.5f));

            yield return new WaitForSeconds(0.5f); 

        }
        else
        {
            yield return new WaitForSeconds(seconds);
        }
        if (isDay)
        {
            print("DayIntro");
            GameManager.instance.DayIntro();
        }
        Destroy(gameObject);
    }
    IEnumerator FadeText(TMP_Text text, float duration)
{
    float startAlpha = text.color.a;
    for (float t = 0; t < duration; t += Time.deltaTime)
    {
        float normalized = t / duration;
        Color c = text.color;
        c.a = Mathf.Lerp(startAlpha, 0f, normalized);
        text.color = c;
        yield return null;
    }
    Color final = text.color;
    final.a = 0f;
    text.color = final;
}
}

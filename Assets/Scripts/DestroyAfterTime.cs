using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float seconds = 1.0f;
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
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}

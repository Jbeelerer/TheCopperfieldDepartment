using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grabbable : MonoBehaviour
{
    private bool isGrabbed = false;
    Transform position;
    private Rigidbody rb;
    public void Grab(Transform pos)
    {
        if (isGrabbed)
        {
            shoot(Vector3.forward);
        }
        else
        {
            transform.localPosition = Vector3.zero;
            position = pos;
            transform.SetParent(pos);
        }
        ToggleGrab();
    }
    private void ToggleGrab()
    {
        rb.isKinematic = !isGrabbed;
        rb.freezeRotation = !isGrabbed;
        isGrabbed = !isGrabbed;
    }
    public void shoot(Vector3 direction)
    {
        if (!isGrabbed)
        {
            return;
        }
        isGrabbed = false;
        transform.SetParent(null);
        rb.isKinematic = true;
        rb.freezeRotation = false;
        rb.AddForce(direction * 1000);
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        if (isGrabbed)
        {
            transform.localPosition = Vector3.zero;
            // transform.position = position.position;
        }

    }
}

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
        gameObject.layer = 2;
        transform.localPosition = Vector3.zero;
        position = pos;
        transform.SetParent(pos);
        rb.isKinematic = true;
        rb.freezeRotation = true;
        isGrabbed = true;

    }
    public void shoot(Vector3 direction)
    {
        if (!isGrabbed)
        {
            return;
        }
        transform.SetParent(null);
        transform.rotation = Quaternion.identity;
        rb.freezeRotation = false;
        gameObject.layer = 0;
        isGrabbed = false;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * 1000);
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isGrabbed)
        {
            transform.localPosition = Vector3.zero;
            // transform.position = position.position;
        }

    }
}

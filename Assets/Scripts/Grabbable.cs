using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Grabbable : MonoBehaviour
{
    private bool isGrabbed = false;
    Transform position;
    private Rigidbody rb;
    [SerializeField] private string objectType = "";
    [SerializeField] private bool isKey = false;
    [SerializeField] private float rotationOffset = 0;
    [SerializeField] private float positionOffset = 0;
    [SerializeField] private float force = 1000;
    public string GetKey()
    {
        if(isKey){
            return objectType;
        }
        return "";  
    }
    public string SetKey(string key)
    {
        objectType = key;
        return objectType;
    }
    public void Grab(Transform pos)
    {
        gameObject.layer = 2;
        transform.localPosition = Vector3.zero + Vector3.up*positionOffset;
        transform.localRotation = Quaternion.identity * Quaternion.Euler(0, 0,rotationOffset);
        position = pos;
        transform.SetParent(pos); 
        rb.isKinematic = true;
        rb.freezeRotation = true;
        GetComponent<Collider>().enabled = false;
        isGrabbed = true;

    }
    public void shoot(Vector3 direction)
    {
        if (!isGrabbed)
        {
            return;
        }        
        GetComponent<Collider>().enabled = true;

        transform.SetParent(null);
        transform.rotation = Quaternion.identity;
        rb.freezeRotation = false;
        gameObject.layer = 0;
        isGrabbed = false;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * force);
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

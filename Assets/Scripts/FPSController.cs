using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;

    public float lookSpeed = 4f;
    public float lookXLimit = 45f;
    public float interactionReach = 3f;


    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    private GameObject currentSelectedObject;


    CharacterController characterController;

    [SerializeField] private GameObject inputOverlay;
    private TextMeshProUGUI inputOverlayText;

    private GameObject selectedPinboardElement;
    [SerializeField] private GameObject thread;
    private LineRenderer currentThread;

    // TODO: Remove pinboard when OS is ready
    [SerializeField] public Pinboard pinboard;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        // Hide the input overlay 
        if (inputOverlay != null)
        {
            inputOverlay.SetActive(false);
            inputOverlayText = inputOverlay.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    void Update()
    {

        #region Handles Movment
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? walkSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? walkSpeed * Input.GetAxis("Horizontal") : 0;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        #endregion

        #region Handles Rotation

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        #endregion

        #region Handles Interaction

        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (Vector3.Distance(hit.collider.gameObject.transform.position, transform.position) <= interactionReach)
            {
                if (hit.collider.gameObject.tag == "Interactable")
                {
                    inputOverlay.SetActive(true);
                    // Set the text based on the object
                    switch (hit.collider.gameObject.name)
                    {
                        case "pinboard":
                            inputOverlayText.text = "";
                            break;
                        case "Button":
                            inputOverlayText.text = "press button";
                            break;
                        case "pinboardElement(Clone)":
                            inputOverlayText.text = "click to move Element";
                            break;
                        case "PC":
                            inputOverlayText.text = "Click to add Element";
                            break;
                        default:
                            inputOverlayText.text = "";
                            break;
                    }
                    currentSelectedObject = hit.collider.gameObject;
                }
                else
                {
                    inputOverlay.SetActive(false);
                    currentSelectedObject = null;
                }
            }
            else
            {
                inputOverlay.SetActive(false);
                currentSelectedObject = null;
            }
        }


        if (Input.GetMouseButtonDown(0) && currentSelectedObject != null)
        {
            if (currentThread != null && currentSelectedObject.name == "pinboardElement(Clone)")
            {

                currentSelectedObject.GetComponent<PinboardElement>().AddEndingThreads(currentThread);
                currentThread = null;
            }
            else if (selectedPinboardElement != null)
            {
                // change back to default layer so it can be selected again
                selectedPinboardElement.gameObject.layer = 0;
                selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(false);
                selectedPinboardElement = null;
            }
            else
            {
                switch (currentSelectedObject.name)
                {
                    case "pinboard":
                        break;
                    case "pinboardElement(Clone)":
                        selectedPinboardElement = currentSelectedObject;
                        selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(true);
                        // change to ignore raycast layer so it won't overflow the pinboard
                        selectedPinboardElement.gameObject.layer = 2;
                        break;
                    case "PC":
                        pinboard.AddPin();
                        break;
                    default:
                        break;
                }
            }
        }
        if (Input.GetMouseButtonDown(1) && currentSelectedObject != null && currentSelectedObject.name == "pinboardElement(Clone)")
        {
            currentThread = Instantiate(thread, pinboard.transform).GetComponent<LineRenderer>();
            currentThread.SetPosition(0, currentSelectedObject.transform.GetChild(0).position);
            currentSelectedObject.GetComponent<PinboardElement>().AddStartingThread(currentThread);
        }
        // handle moving ponboardElement position
        if (selectedPinboardElement != null && currentSelectedObject != null)
        {
            selectedPinboardElement.transform.position = new Vector3(selectedPinboardElement.transform.position.x, hit.point.y, hit.point.z);
        }
        // handle moving thread position
        if (currentThread != null)
        {
            currentThread.SetPosition(1, hit.point);
            if (currentSelectedObject == null)
            {
                Destroy(currentThread);
            }
        }

        #endregion

    }

}

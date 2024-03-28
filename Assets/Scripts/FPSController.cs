using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Camera playerCamera;
    public Transform cameraObject;
    public float walkSpeed = 6f;

    public float lookSpeed = 4f;
    public float lookXLimit = 45f;
    public float interactionReach = 3f;

    private bool deletionMode = false;

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

    private float hoverStart = -1f;
    private bool detailMode = false;

    private AudioSource audioSource;

    private bool frozen = false;

    private ComputerControls computerControls;

    [SerializeField] private AudioClip threadCuttingSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip deleteSound;

    // TODO: Remove pinboard when OS is ready
    [SerializeField] public Pinboard pinboard;

    private float removingTime = -1f;

    private Animator selectedPenAnim;
    private Vector3 penPos;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hide the input overlay 
        if (inputOverlay != null)
        {
            inputOverlay.SetActive(false);
            inputOverlayText = inputOverlay.GetComponentInChildren<TextMeshProUGUI>();
        }

        computerControls = GameObject.Find("DesktopInterface").GetComponent<ComputerControls>();
    }
    private void FixedUpdate()
    {
        if (hoverStart != -1f)
        {
            hoverStart += Time.deltaTime;
        }
        if (hoverStart > 1f)
        {
            hoverStart = -1f;
        }
    }
    void Update()
    {
        // on e key pressed
        if (Input.GetKeyDown(KeyCode.E) && frozen)
        {
            //Cursor.lockState = Cursor.visible ? CursorLockMode.Locked : CursorLockMode.None;
            //Cursor.visible = !Cursor.visible;
            frozen = false;
            cameraObject.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            computerControls.ToggleCursor();
        }

        if (!frozen)
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
                cameraObject.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
            }

            #endregion

            #region Handles Interaction

            RaycastHit hit;
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButtonUp(0))
            {
                removingTime = -1f;
            }

            // if removingTime is the default value it is active and the currentselectedObject shouldn't change
            if (Physics.Raycast(ray, out hit) && removingTime == -1)
            {
                if (Vector3.Distance(hit.collider.gameObject.transform.position, transform.position) <= interactionReach && hit.collider.gameObject.tag == "Interactable")
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
                            print(!detailMode && hoverStart > 0.4f);
                            print(hoverStart > 0.4f);
                            if (!detailMode && hoverStart > 0.7f)
                            {
                                AdditionalInfoBoard aib = transform.GetChild(0).Find("MoreInfo").GetComponent<AdditionalInfoBoard>();
                                aib.ShowInfo(true);
                                print(hit.collider.gameObject.GetComponent<PinboardElement>());
                                print(hit.collider.gameObject.GetComponent<PinboardElement>().GetContent());
                                aib.SetContent(hit.collider.gameObject.GetComponent<PinboardElement>().GetContent());

                                detailMode = true;
                            }
                            else if (!detailMode && hoverStart == -1f)
                                hoverStart = 0;
                            break;
                        case "PC":
                            inputOverlayText.text = "Click to add Element";
                            break;
                        default:
                            inputOverlayText.text = "";
                            break;
                    }
                    currentSelectedObject = hit.collider.gameObject;
                    if (hit.collider.gameObject.name != "pinboardElement(Clone)")
                    {
                        hoverStart = -1;
                        detailMode = false;
                        transform.GetChild(0).Find("MoreInfo").GetComponent<AdditionalInfoBoard>().ShowInfo(false);
                    }
                }
                else
                {
                    inputOverlay.SetActive(false);
                    if (selectedPinboardElement != null)
                    {
                        selectedPinboardElement.gameObject.layer = 0;
                        selectedPinboardElement.GetComponentInParent<Animator>().SetBool("pinNormal", true);
                        selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(false);
                        PlayReverseAudio(pickupSound);
                        selectedPinboardElement = null;
                    }
                    currentSelectedObject = null;
                }
            }
            // default val
            else if (removingTime != -1)
            {
                removingTime -= Time.deltaTime;
                print(removingTime);
                if (removingTime <= 0)
                {
                    print(currentSelectedObject);
                    currentSelectedObject.transform.parent.GetComponent<PinboardElement>().DeleteElement();
                    audioSource.PlayOneShot(deleteSound);
                    removingTime = -1;
                }
            }

            if (Input.GetMouseButtonDown(0) && currentSelectedObject != null)
            {
                if (currentThread != null && currentSelectedObject.name == "pinboardElement(Clone)")
                {
                    currentSelectedObject.GetComponent<PinboardElement>().AddEndingThreads(currentThread);
                    currentSelectedObject.GetComponent<PinboardElement>().setIsMoving(false);
                    /*  Vector2[] edgeColl = currentThread.GetComponent<EdgeCollider2D>().points;
                      List<Vector2> list = new List<Vector2>();
                      list.Add(currentThread.GetComponent<EdgeCollider2D>().points[0]); 
                      list.Add(currentSelectedObject.transform.GetChild(0).position);
                      currentThread.GetComponent<EdgeCollider2D>().SetPoints(list);*/
                    currentThread = null;
                }
                else if (selectedPinboardElement != null)
                {  // change back to default layer so it can be selected again
                    selectedPinboardElement.gameObject.layer = 0;
                    selectedPinboardElement.GetComponentInParent<Animator>().SetBool("pinNormal", true);
                    selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(false);
                    PlayReverseAudio(pickupSound);
                    selectedPinboardElement = null;
                }
                else
                {
                    // Deselect pen if clicking somewhere, where you can't draw
                    if (selectedPenAnim != null)
                    {
                        print(currentSelectedObject.name);
                    }
                    if (selectedPenAnim != null && !(currentSelectedObject.name == "pin" || currentSelectedObject.name == "Pen" || currentSelectedObject.name == "pinboardElement(Clone)"))
                    {
                        DeselectPen();
                    }
                    switch (currentSelectedObject.name)
                    {
                        case "pinboardElement(Clone)":
                            if (selectedPenAnim != null)
                            {
                                currentSelectedObject.GetComponent<PinboardElement>().annotateCircle();
                                selectedPenAnim.SetTrigger("circle");
                                freezeForX(0.5f);
                            }
                            else
                            {
                                currentSelectedObject.GetComponentInParent<Animator>().SetBool("pinNormal", false);
                                selectedPinboardElement = currentSelectedObject;
                                selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(true);
                                audioSource.PlayOneShot(pickupSound);
                                // change to ignore raycast layer so it won't overflow the pinboard
                                selectedPinboardElement.gameObject.layer = 2;
                            }
                            break;
                        case "CurvedScreen":
                            cameraObject.gameObject.SetActive(false);
                            frozen = true;
                            Cursor.lockState = CursorLockMode.Confined;
                            Cursor.visible = false;
                            computerControls.ToggleCursor();
                            inputOverlay.gameObject.SetActive(false);
                            break;
                        case "threadCollider":
                            audioSource.PlayOneShot(threadCuttingSound);
                            Destroy(hit.collider.transform.parent.gameObject);
                            break;
                        case "pin":
                            currentSelectedObject.GetComponentInParent<Animator>().SetTrigger("remove");
                            audioSource.PlayOneShot(pickupSound);
                            removingTime = 0.5f;
                            break;
                        case "Pen":
                            penPos = currentSelectedObject.transform.position;
                            selectedPenAnim = currentSelectedObject.GetComponentInChildren<Animator>();
                            selectedPenAnim.transform.parent.gameObject.layer = 2;
                            selectedPenAnim.SetBool("pickedup", true);
                            break;
                        default:
                            break;
                    }
                }
            }
            if (Input.GetMouseButtonDown(1) && currentSelectedObject != null && currentSelectedObject.name == "pinboardElement(Clone)")
            {
                if (selectedPenAnim != null)
                {
                    currentSelectedObject.GetComponent<PinboardElement>().annotateStrikeThrough();
                    selectedPenAnim.SetTrigger("cross");
                    freezeForX(0.5f);
                }
                else
                {
                    currentThread = Instantiate(thread, pinboard.transform).GetComponent<LineRenderer>();
                    currentThread.SetPosition(0, currentSelectedObject.transform.GetChild(0).position);
                    currentSelectedObject.GetComponent<PinboardElement>().AddStartingThread(currentThread);
                }
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

            if (selectedPenAnim != null)
            {
                selectedPenAnim.transform.parent.position = new Vector3(pinboard.transform.position.x, hit.point.y, hit.point.z);
            }

            #endregion

        }
    }
    public void PlayReverseAudio(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.pitch = -1;
        audioSource.time = audioSource.clip.length - 0.01f;
        audioSource.Play();
        StartCoroutine(resetAudio(audioSource.clip.length));
    }

    public IEnumerator resetAudio(float length)
    {
        yield return new WaitForSeconds(length);
        audioSource.pitch = 1;
        audioSource.time = 0;
    }

    private void DeselectPen()
    {
        print("deselect");
        selectedPenAnim.transform.parent.gameObject.layer = 0;
        selectedPenAnim.SetBool("pickedup", false);
        selectedPenAnim.transform.parent.position = penPos;
        selectedPenAnim = null;
    }

    public IEnumerator freezeForX(float X)
    {
        frozen = true;
        yield return new WaitForSeconds(X);
        frozen = false;
    }


}

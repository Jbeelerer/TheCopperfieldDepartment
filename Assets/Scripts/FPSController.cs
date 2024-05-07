using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Events;


[System.Serializable]
public class DeletionEvent : UnityEvent<ScriptableObject> { }


[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Camera playerCamera;
    public Transform cameraObject;
    public float walkSpeed = 6f;

    public float lookSpeed = 4f;
    public float lookXLimit = 50f;
    public float interactionReach = 3f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    private GameObject currentSelectedObject;
    private GameObject lastSelectedObject;

    // used to determine if player still looks at board, otherwise the pinboardElement will be deselected
    private string nameOfThingLookedAt = "";

    CharacterController characterController;

    [SerializeField] private InputOverlay inputOverlay;
    private TextMeshProUGUI inputOverlayText;

    private GameObject selectedPinboardElement;
    [SerializeField] private GameObject thread;
    [SerializeField] private GameObject connectionPrefab;
    private LineRenderer currentThread;

    private float hoverStart = -1f;
    // in detailmode the additional info board is shown
    private bool detailMode = false;
    private ComputerControls computerControls;
    private GameObject computerCam;
    private GameObject calendarCam;

    //audio
    private AudioSource audioSource;
    [SerializeField] private AudioClip threadCuttingSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip deleteSound;

    private Narration narration;


    // TODO: Remove pinboard when OS is ready
    [SerializeField] public Pinboard pinboard;

    private float removingTime = -1f;

    private Animator selectedPenAnim;
    private Vector3 penPos;

    private GameManager gm;

    private bool onHoldDown = false;

    public DeletionEvent OnPinDeletion;

    private Vector3 originalPosition;

    // TODO change in gm directly, for now not fucking with computercontrolls @alex <3
    public void SetIsFrozen(bool isFrozen)
    {
        gm.SetGameState(isFrozen ? GameState.Frozen : GameState.Playing);
    }

    void Start()
    {
        gm = GameManager.instance;
        audioSource = GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        inputOverlay = gameObject.GetComponentInChildren<InputOverlay>();

        computerControls = GameObject.Find("DesktopInterface").GetComponent<ComputerControls>();
        gm.SetStartTransform(transform);
        narration = GetComponentInChildren<Narration>();
        computerCam = GameObject.Find("ComputerCam");
        calendarCam = GameObject.Find("CalendarCam");
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

        if (removingTime != -1)
        {
            removingTime -= Time.deltaTime;
            if (removingTime <= 0)
            {
                PinboardElement pe = currentSelectedObject.name == "pin" ? currentSelectedObject.transform.parent.GetComponent<PinboardElement>() : selectedPinboardElement.GetComponent<PinboardElement>();

                if (pe.GetIfDeletable())
                {
                    OnPinDeletion?.Invoke(pe.GetContent());
                    pe.DeleteElement();
                    audioSource.PlayOneShot(deleteSound);
                }
                else
                {
                    pe.transform.position = originalPosition;
                    PlayReverseAudio(pickupSound);
                    pe.GetComponentInParent<Animator>().SetBool("pinNormal", true);
                    pe.gameObject.layer = 0;
                }

                removingTime = -1;
                inputOverlay.stopHold();
                selectedPinboardElement = null;
            }
        }
    }
    void Update()
    {
        // on e key pressed ExitPC is "E"
        if (Input.GetButtonDown("ExitPC") && gm.isFrozen())
        {
            if (lastSelectedObject)
            {
                computerControls.LeaveComputer();
            }
            handleCamera(cameraObject.gameObject);
        }

        if (!gm.isFrozen())
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
            // TODO: Add a freeze when in the deadzone, but only towards the direction currently moving, so you can cancel by going back
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

            // if removingTime is the default value it is active and the currentselectedObject shouldn't change
            #region Look at things
            if (Physics.Raycast(ray, out hit) && !onHoldDown)
            {
                if (Vector3.Distance(hit.collider.gameObject.transform.position, transform.position) <= interactionReach && hit.collider.gameObject.tag == "Interactable")
                {
                    nameOfThingLookedAt = hit.collider.gameObject.name;
                    // if lastSelectedObject the player is grabing a thread and shouldn't be able see anything else
                    if (lastSelectedObject == null)
                    {
                        // handle interaction, while just looking
                        switch (nameOfThingLookedAt)
                        {
                            case "PinboardModel":
                                if (selectedPinboardElement == null && currentThread == null)//TODO: add Pen exxption
                                {
                                    inputOverlay.SetIcon("defaultIcon");
                                }
                                if (selectedPinboardElement != null)
                                {
                                    if (removingTime != -1)
                                    {
                                        removingTime = -1f;
                                        inputOverlay.stopHold();
                                        inputOverlay.SetIcon("handClosed");
                                    }
                                }
                                break;
                            case "DeadZone":
                                if (selectedPinboardElement != null && removingTime == -1)
                                {
                                    inputOverlay.SetIcon("trash", true);
                                    // the typical animation makes no sense here TODO: Maybe add a new animation
                                    audioSource.PlayOneShot(pickupSound);
                                    removingTime = 1f;
                                    inputOverlay.startHold(removingTime);
                                }
                                break;
                            case "Button":
                                break;
                            case "pinboardElement(Clone)":
                                PinboardElement pe = hit.collider.gameObject.GetComponent<PinboardElement>();
                                inputOverlay.SetIcon("handOpen");
                                // highlight and scale the pinboardElement
                                pe.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                                pe.HighlightElement(true);
                                if (selectedPenAnim != null)
                                {
                                    inputOverlay.SetIcon("pen");
                                }
                                else if (!detailMode && hoverStart > 0.5f && pe.GetIfHasInfo())
                                {
                                    AdditionalInfoBoard aib = transform.GetChild(0).Find("MoreInfo").GetComponent<AdditionalInfoBoard>();
                                    aib.ShowInfo(true);
                                    aib.SetContent(pe.GetContent());
                                    detailMode = true;
                                }
                                else if (!detailMode && hoverStart == -1f)
                                    hoverStart = 0;
                                break;
                            case "CurvedScreen":
                            case "Calendar":
                                inputOverlay.SetIcon("inspect");
                                break;
                            case "pin":
                                inputOverlay.SetIcon("trash");
                                break;
                            case "threadCollider":
                                inputOverlay.SetIcon("scissors");
                                break;
                            case "Door":
                                inputOverlay.SetIcon("exit");
                                break;
                            case "Pen":
                                inputOverlay.SetIcon("handOpen");
                                break;
                            default:
                                break;
                        }
                    }
                    if (hit.collider.gameObject.name != "pinboardElement(Clone)")
                    {
                        // undoes highlight and scaling of the pinboardElement
                        if (currentSelectedObject != null && selectedPinboardElement == null && currentSelectedObject.name == "pinboardElement(Clone)")
                        {
                            currentSelectedObject.transform.localScale = new Vector3(1, 1, 1);
                            currentSelectedObject.GetComponent<PinboardElement>().HighlightElement(false);
                        }
                        hoverStart = -1;
                        detailMode = false;
                        transform.GetChild(0).Find("MoreInfo").GetComponent<AdditionalInfoBoard>().ShowInfo(false);
                    }
                    // dont reset the currentSelectedObject if the player is removing a pinboardElement
                    if (nameOfThingLookedAt != "DeadZone")
                    {
                        currentSelectedObject = hit.collider.gameObject;
                    }
                }
                else if (currentSelectedObject != null && removingTime == -1)
                {
                    DeselectPen();
                    inputOverlay.SetIcon("");
                    if (selectedPinboardElement != null)
                    {
                        selectedPinboardElement.gameObject.layer = 0;
                        selectedPinboardElement.GetComponentInParent<Animator>().SetBool("pinNormal", true);
                        selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(false);
                        PlayReverseAudio(pickupSound);
                        selectedPinboardElement = null;
                        inputOverlay.SetIcon("defaultIcon");
                    }
                    currentSelectedObject = null;
                }
            }

            #endregion
            #region Handle left click
            if (Input.GetMouseButtonDown(0) && currentSelectedObject != null)
            {
                //Connect threat to pinboardElement
                if (currentThread != null)
                {
                    if (currentSelectedObject.name == "pinboardElement(Clone)")
                    {
                        currentSelectedObject.GetComponent<PinboardElement>().AddEndingThreads(currentThread);
                        currentSelectedObject.GetComponent<PinboardElement>().setIsMoving(false);
                        string connectionText = gm.checkForConnectionText(currentSelectedObject.GetComponent<PinboardElement>().GetContent(), lastSelectedObject.GetComponent<PinboardElement>().GetContent());
                        if (connectionText != "")
                        {
                            GameObject connectionO = Instantiate(connectionPrefab, currentThread.transform);
                            connectionO.transform.position = currentThread.transform.GetChild(0).position - new Vector3(0, 0.05f, 0);
                            connectionO.GetComponentInChildren<TextMeshProUGUI>().text = connectionText;
                        }
                    }
                    else
                    {
                        // Reset threat
                        Destroy(currentThread);
                    }
                    lastSelectedObject = null;
                    currentThread = null;
                }
                // places the pinboardElement back to the pinboard
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
                    if (selectedPenAnim != null && !(currentSelectedObject.name == "pin" || currentSelectedObject.name == "Pen" || currentSelectedObject.name == "pinboardElement(Clone)"))
                    {
                        DeselectPen();
                    }
                    switch (currentSelectedObject.name)
                    {
                        case "pinboardElement(Clone)":
                            if (selectedPenAnim != null)
                            {
                                StartCoroutine(PlayPenAnimation("circle"));
                            }
                            else
                            {
                                originalPosition = currentSelectedObject.transform.position;
                                inputOverlay.SetIcon("handClosed");
                                currentSelectedObject.GetComponentInParent<Animator>().SetBool("pinNormal", false);
                                selectedPinboardElement = currentSelectedObject;
                                selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(true);
                                audioSource.PlayOneShot(pickupSound);
                                // change to ignore raycast layer so it won't overflow the pinboard
                                selectedPinboardElement.gameObject.layer = 2;
                            }
                            break;
                        case "CurvedScreen":
                            handleCamera(computerCam);
                            computerControls.ToggleCursor();
                            break;
                        case "threadCollider":
                            audioSource.PlayOneShot(threadCuttingSound);
                            Destroy(hit.collider.transform.parent.gameObject);
                            break;
                        case "pin":
                            onHoldDown = true;
                            currentSelectedObject.GetComponentInParent<Animator>().SetTrigger("remove");
                            audioSource.PlayOneShot(pickupSound);
                            removingTime = 0.5f;
                            inputOverlay.startHold(removingTime);
                            break;
                        case "Pen":
                            penPos = currentSelectedObject.transform.position;
                            selectedPenAnim = currentSelectedObject.GetComponentInChildren<Animator>();
                            selectedPenAnim.transform.parent.gameObject.layer = 2;
                            selectedPenAnim.SetBool("pickedup", true);
                            break;
                        case "Calendar":
                            handleCamera(calendarCam);
                            break;
                        case "Door":
                            if (gm.GetAnswerCommited())
                            {
                                gm.setNewDay();
                                transform.position = gm.GetStartPosition();
                                transform.rotation = gm.GetStartRotation();
                            }
                            else
                            {
                                // Todo: implement player feedback
                                print("Answer not commited");
                                narration.Say("notLeaving");
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0) && onHoldDown)
            {
                onHoldDown = false;
                inputOverlay.stopHold();
                removingTime = -1f;
            }
            #endregion
            #region Handle right click
            if (Input.GetMouseButtonDown(1) && currentSelectedObject != null && currentSelectedObject.name == "pinboardElement(Clone)")
            {
                if (selectedPenAnim != null)
                {
                    StartCoroutine(PlayPenAnimation("cross"));
                }
                else
                {
                    currentThread = Instantiate(thread, currentSelectedObject.transform.parent.transform).GetComponent<LineRenderer>();
                    // TODO: The reason for 4 positions, is so it won't clip through the pinboardElement, not completely implemented yet
                    currentThread.positionCount = 4;
                    currentThread.SetPosition(0, currentSelectedObject.transform.GetChild(0).position);
                    currentThread.SetPosition(1, currentSelectedObject.transform.GetChild(0).position);
                    currentThread.transform.GetChild(0).gameObject.SetActive(false);
                    currentSelectedObject.GetComponent<PinboardElement>().AddStartingThread(currentThread);
                    lastSelectedObject = currentSelectedObject;
                    inputOverlay.SetIcon("handThread");
                }
            }
            #endregion
            #region handle selected element movement
            // handle moving pinboardElement position 
            if (selectedPinboardElement != null && currentSelectedObject != null && (nameOfThingLookedAt == "PinboardModel" || nameOfThingLookedAt == "DeadZone"))
            {
                selectedPinboardElement.transform.position = new Vector3(selectedPinboardElement.transform.position.x, hit.point.y, hit.point.z);
            }
            // handle moving thread position
            if (currentThread != null)
            {
                currentThread.SetPosition(2, hit.point);
                currentThread.SetPosition(3, hit.point);

                if (currentSelectedObject == null)
                {
                    Destroy(currentThread);
                }
            }
            // handle pen position
            if (selectedPenAnim != null)
            {
                selectedPenAnim.transform.parent.position = new Vector3(selectedPenAnim.transform.parent.position.x, hit.point.y, hit.point.z);
            }
            #endregion
            #endregion

        }
        else
        {
            // TODO: This is not a good way to handle Worldspace UI, but the buttons are not interactable otherwise
            // IF YOU KNOW WHY PLEASE FIX!!!!!!! (then you can also remove the collider from the button)
            RaycastHit hit;
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.name == "Day(Clone)" && hit.collider.gameObject.GetComponent<UnityEngine.UI.Button>().interactable)
                {
                    hit.collider.gameObject.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                }
            }
        }
    }

    private void handleCamera(GameObject camera)
    {
        computerCam.SetActive(false);
        if (calendarCam)
        {
            calendarCam.SetActive(false);
            calendarCam.transform.parent.parent.GetComponent<Collider>().enabled = true;
        }
        cameraObject.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (camera != cameraObject.gameObject)
        {
            gm.SetGameState(GameState.Frozen);
            inputOverlay.SetIcon("");
            Cursor.lockState = CursorLockMode.Confined;
            if (camera == calendarCam)
            {
                Cursor.visible = true;
                calendarCam.transform.parent.parent.GetComponent<Collider>().enabled = false;
            }
        }
        else
        {
            gm.SetGameState(GameState.Playing);
        }
        camera.SetActive(true);
    }

    // TODO: Maybe create an audio manager for all these things
    public void PlayReverseAudio(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.pitch = -1;
        audioSource.time = audioSource.clip.length - 0.01f;
        audioSource.Play();
        StartCoroutine(ResetAudio(audioSource.clip.length));
    }
    public IEnumerator ResetAudio(float length)
    {
        yield return new WaitForSeconds(length);
        audioSource.pitch = 1;
        audioSource.time = 0;
    }

    private void DeselectPen()
    {
        if (selectedPenAnim == null)
            return;
        selectedPenAnim.transform.parent.gameObject.layer = 0;
        selectedPenAnim.SetBool("pickedup", false);
        selectedPenAnim.transform.parent.position = penPos;
        selectedPenAnim = null;
    }

    public IEnumerator PlayPenAnimation(string animName)
    {
        if (!gm.isFrozen())
        {
            PinboardElement pe = currentSelectedObject.GetComponent<PinboardElement>();
            gm.SetGameState(GameState.Frozen);
            // clear if the state is the same
            if (animName == "circle" && pe.GetAnnotationType() == AnnotationType.Circle || animName == "cross" && pe.GetAnnotationType() == AnnotationType.StrikeThrough)
            {
                selectedPenAnim.SetTrigger("clear");
                yield return new WaitForSeconds(selectedPenAnim.runtimeAnimatorController.animationClips[5].length);
                pe.SetAnnotationType(AnnotationType.None);
            }
            else
            {
                selectedPenAnim.SetTrigger(animName);
                yield return new WaitForSeconds(selectedPenAnim.runtimeAnimatorController.animationClips[animName == "circle" ? 3 : 4].length);
                if (animName == "circle")
                {
                    pe.SetAnnotationType(AnnotationType.Circle);
                }
                else
                {
                    pe.SetAnnotationType(AnnotationType.StrikeThrough);
                }
            }
            //pe.SetAnnotationType(animName == "circle" ? AnnotationType.Circle : animName == "cross" ? AnnotationType.StrikeThrough : AnnotationType.StrikeThrough);
            gm.SetGameState(GameState.Playing);
        }
    }

}

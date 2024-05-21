using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Events;
using System;
using Cinemachine;


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
    public float interactionReach = 4f;

    public float hoverDuration = 1f;

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
    private ComputerControls computerControls;
    [SerializeField] private AudioClip threadCuttingSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip deleteSound;

    private Narration narration;

    private AdditionalInfoBoard additionalInfoBoard;


    // TODO: Remove pinboard when OS is ready
    [SerializeField] public Pinboard pinboard;

    private float removingTime = -1f;

    private Animator selectedPenAnim;
    private Vector3 penPos;

    private GameManager gm;

    private AudioManager am;

    private bool onHoldDown = false;

    public DeletionEvent OnPinDeletion;

    private Vector3 originalPosition;

    private List<GameObject> foundConnections = new List<GameObject>();

    private float rotationOffset = 0;

    private GameObject flaggedPE;
    private GameObject flaggedThread;

    [SerializeField] private Transform grabPos;

    private Grabbable grabbedObject;

    public void ResetFoundConnections()
    {
        foreach (GameObject go in foundConnections)
        {
            Destroy(go);
        }
        foundConnections.Clear();
    }

    void Start()
    {
        gm = GameManager.instance;
        am = AudioManager.instance;
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        inputOverlay = gameObject.GetComponentInChildren<InputOverlay>();

        computerControls = GameObject.Find("DesktopInterface").GetComponent<ComputerControls>();
        gm.SetStartTransform(transform);
        narration = GetComponentInChildren<Narration>();
        additionalInfoBoard = transform.GetChild(0).Find("MoreInfo").GetComponent<AdditionalInfoBoard>();
        gm.OnNewDay.AddListener(ResetFoundConnections);
        gm.OnNewDay.AddListener(ResetPlayer);
    }
    private void FixedUpdate()
    {
        if (hoverStart != -1f)
        {
            hoverStart += Time.deltaTime;
        }
        if (hoverStart > hoverDuration + 0.5f)
        {
            hoverStart = -1f;
        }

        if (removingTime != -1)
        {
            removingTime -= Time.deltaTime;
            if (removingTime <= 0)
            {
                if (currentSelectedObject.transform.parent.name == "Connection")
                {
                    Destroy(currentSelectedObject.transform.parent.gameObject);
                    return;
                }
                PinboardElement pe = currentSelectedObject.name == "pin" ? currentSelectedObject.transform.parent.GetComponent<PinboardElement>() : selectedPinboardElement.GetComponent<PinboardElement>();

                if (pe.GetIfDeletable())
                {
                    OnPinDeletion?.Invoke(pe.GetContent());
                    pe.DeleteElement();
                    am.PlayAudio(deleteSound);
                }
                else
                {
                    pe.transform.position = originalPosition;
                    am.PlayReverseAudio(pickupSound);
                    pe.GetComponentInParent<Animator>().SetBool("pinNormal", true);
                    pe.gameObject.layer = 0;
                }

                removingTime = -1;
                inputOverlay.stopHold();
                selectedPinboardElement = null;
            }
        }
    }

    public void ResetCameraRotation(Quaternion rotation, bool instant = false)
    {
        if (instant)
            transform.rotation = rotation;
        else
        {
            if (rotation != Quaternion.identity)
            {
                // transform.rotation = rotation;
                StartCoroutine(LerpToRotation(rotation));
            }
        }
        //GetComponentInChildren<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = rotation.eulerAngles.y;
    }
    void Update()
    {
        // on e key pressed ExitPC is "E"
        if (Input.GetButtonDown("ExitPC") && gm.isFrozen())
        {
            if (gm.GetGameState() == GameState.OnPC)
            {
                computerControls.LeaveComputer();
            }
            else
            {

                gm.SetGameState(GameState.Playing);
            }
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
                rotationX += (-Input.GetAxis("Mouse Y")) * lookSpeed;
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
                if (Vector3.Distance(hit.collider.gameObject.transform.position, transform.position) <= interactionReach)
                {
                    if (hit.collider.gameObject.tag == "Grabbable")
                    {
                        nameOfThingLookedAt = hit.collider.gameObject.name;
                        inputOverlay.SetIcon("handOpen");
                    }
                    else if (hit.collider.gameObject.tag == "Interactable")
                    {
                        nameOfThingLookedAt = hit.collider.gameObject.name;
                        // if lastSelectedObject the player is grabing a thread and shouldn't be able see anything else
                        if (lastSelectedObject == null)
                        {
                            // handle interaction, while just looking
                            switch (nameOfThingLookedAt)
                            {
                                case "PinboardModel":
                                    // interactionReach = 10f;
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
                                        am.PlayAudio(pickupSound);
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
                                    pe.HighlightElement(true);
                                    if (selectedPenAnim != null)
                                    {
                                        inputOverlay.SetIcon("pen");
                                    }
                                    else if (pe.GetIfHasInfo())
                                    {
                                        additionalInfoBoard.ShowInfo(true, pe.GetContent());
                                    }
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
                                case "Phone":
                                    inputOverlay.SetIcon("inspect");
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (hit.collider.gameObject.name != "pinboardElement(Clone)")
                        {
                            // undoes highlight and scaling of the pinboardElement
                            if (currentSelectedObject != null && currentSelectedObject.name == "pinboardElement(Clone)")
                            {
                                if (selectedPinboardElement == null)
                                {
                                    currentSelectedObject.GetComponent<PinboardElement>().HighlightElement(false);
                                }
                                additionalInfoBoard.CancelPreview();
                            }
                        }
                        // highlight when hovering with a thread over a pinboardElement
                        else if (lastSelectedObject != null && hit.collider.gameObject.name == "pinboardElement(Clone)")
                        {
                            hit.collider.gameObject.GetComponent<PinboardElement>().HighlightElement(true);
                        }
                        // dont reset the currentSelectedObject if the player is removing a pinboardElement
                        if (nameOfThingLookedAt != "DeadZone")
                        {
                            currentSelectedObject = hit.collider.gameObject;
                        }
                    }
                }
                else if (currentSelectedObject != null && removingTime == -1)
                {
                    DeselectPen();
                    inputOverlay.ChangeIconIfDifferent("");
                    if (selectedPinboardElement != null)
                    {
                        selectedPinboardElement.gameObject.layer = 0;
                        selectedPinboardElement.GetComponentInParent<Animator>().SetBool("pinNormal", true);
                        selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(false);
                        am.PlayReverseAudio(pickupSound);
                        selectedPinboardElement = null;
                    }
                    currentSelectedObject = null;
                    if (currentThread != null)
                    {
                        TryToPlaceThread("");
                    }
                    //interactionReach = 3f;
                }
                else
                {
                    //interactionReach = 3f;
                    inputOverlay.ChangeIconIfDifferent("");
                }
            }

            #endregion
            #region Handle left click
            if (Input.GetMouseButtonDown(0))
            {

                if (hit.collider.gameObject.tag == "Grabbable")
                {
                    grabbedObject = hit.collider.gameObject.GetComponent<Grabbable>();
                    grabbedObject.Grab(grabPos);
                }
                else if (grabbedObject != null)
                {
                    grabbedObject.Grab(grabPos);
                    grabbedObject = null;
                }

                else if (currentSelectedObject != null)
                {
                    //Connect threat to pinboardElement
                    if (currentThread != null)
                    {
                        TryToPlaceThread(currentSelectedObject.name);
                    }
                    // places the pinboardElement back to the pinboard
                    else if (selectedPinboardElement != null)
                    {  // change back to default layer so it can be selected again
                        selectedPinboardElement.gameObject.layer = 0;
                        selectedPinboardElement.GetComponentInParent<Animator>().SetBool("pinNormal", true);
                        selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(false);
                        am.PlayReverseAudio(pickupSound);
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
                                    am.PlayAudio(pickupSound);
                                    // change to ignore raycast layer so it won't overflow the pinboard
                                    selectedPinboardElement.gameObject.layer = 2;
                                }
                                break;
                            case "CurvedScreen":
                                gm.SetGameState(GameState.OnPC);
                                inputOverlay.SetIcon("");
                                computerControls.ToggleCursor();
                                break;
                            case "threadCollider":
                                am.PlayAudio(threadCuttingSound);
                                if (hit.collider.transform.parent.gameObject == flaggedThread)
                                {
                                    flaggedPE.GetComponent<PinboardElement>().SetAnnotationType(AnnotationType.None);
                                    flaggedPE = null;
                                    flaggedThread = null;
                                }
                                Destroy(hit.collider.transform.parent.gameObject);
                                break;
                            case "pin":
                                onHoldDown = true;
                                currentSelectedObject.GetComponentInParent<Animator>().SetTrigger("remove");
                                am.PlayAudio(pickupSound);
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
                                inputOverlay.SetIcon("");
                                gm.SetGameState(GameState.OnCalendar);
                                break;
                            case "Phone":
                                inputOverlay.SetIcon("");
                                hit.collider.transform.GetComponent<Phone>().StartCall();
                                break;
                            case "Door":
                                if (gm.GetAnswerCommited())
                                {
                                    inputOverlay.SetIcon("");
                                    gm.setNewDay();
                                    // Reset player position
                                    ResetPlayer();
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
                    if (currentThread != null)
                    {
                        if (currentSelectedObject.name == "pinboardElement(Clone)")
                        {
                            HandleThread();
                        }
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
            }
            // place the thread after stopping to drag
            if (Input.GetMouseButtonUp(1) && currentThread != null)
            {
                if (lastSelectedObject.transform != hit.collider.transform)
                {
                    TryToPlaceThread(nameOfThingLookedAt);
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

    private void ResetPlayer()
    {
        transform.position = gm.GetStartPosition();
        transform.rotation = gm.GetStartRotation();
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

    public void TryToPlaceThread(string underground)
    {
        if (underground == "pinboardElement(Clone)")
        {
            HandleThread();
        }
        else
        {
            // Reset threat
            Destroy(currentThread);
        }
        lastSelectedObject = null;
        currentThread = null;
    }
    public void HandleThread()
    {
        PinboardElement currentPE = currentSelectedObject.GetComponent<PinboardElement>();
        PinboardElement lastPE = lastSelectedObject.GetComponent<PinboardElement>();

        additionalInfoBoard.ShowInfo(false);
        currentPE.AddEndingThreads(currentThread);
        currentPE.setIsMoving(false);
        Connections connection = gm.checkForConnectionText(currentPE.GetContent(), lastPE.GetContent());

        // the suspicios person post it was connected 
        if (!currentPE.GetIfDeletable() || !lastPE.GetIfDeletable())
        {
            if (currentPE.GetContent() is Person || lastPE.GetContent() is Person)
            {
                if (flaggedPE != null)
                {
                    flaggedPE.GetComponent<PinboardElement>().SetAnnotationType(AnnotationType.None);
                }
                if (flaggedThread != null)
                {
                    Destroy(flaggedThread);
                }
                flaggedThread = currentThread.gameObject;
            }

            if (currentPE.GetContent() is Person)
            {
                gm.checkSuspect(currentPE.GetContent() as Person);
                currentPE.SetAnnotationType(AnnotationType.CaughtSuspect);
                flaggedPE = currentSelectedObject;
            }
            else if (lastPE.GetContent() is Person)
            {
                gm.checkSuspect(lastPE.GetContent() as Person);
                lastPE.SetAnnotationType(AnnotationType.CaughtSuspect);
                flaggedPE = lastSelectedObject;
            }
        }

        if (connection != null)
        {
            GameObject connectionO = Instantiate(connectionPrefab, currentThread.transform);
            Color color;
            // handle contradiction color 
            UnityEngine.ColorUtility.TryParseHtmlString(connection.isContradiction ? "#F5867C" : "#F5DB7C", out color);
            connectionO.transform.Find("PostIt").GetComponent<MeshRenderer>().material.color = color;
            connectionO.transform.position = currentThread.transform.GetChild(0).position - new Vector3(0, 0.05f, 0);
            connectionO.GetComponentInChildren<TextMeshProUGUI>().text = connection.text;
            foundConnections.Add(connectionO);
        }
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

    public IEnumerator LerpToRotation(Quaternion targetRotation)
    {
        float time = 0;
        Quaternion startRotation = transform.rotation;
        while (time < 1)
        {
            time += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            //transform.rotation = Quaternion.Euler(Mathf.Lerp(startRotationx, targetRotation.eulerAngles.x, time), Mathf.Lerp(startRotationy, targetRotation.eulerAngles.y, time), 0);
            yield return null;
        }
    }

}

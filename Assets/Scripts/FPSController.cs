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
using SaveSystem;
using UnityEngine.SceneManagement;


[System.Serializable]
public class DeletionEvent : UnityEvent<ScriptableObject> { }


[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public Camera playerCamera;
    public Transform cameraObject;
    public float walkSpeed = 6f;

    public float lookSpeed = 4f;
    public float lookXLimit = 90f;
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
    [SerializeField] private AudioClip chairRollSound;
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip pcAccessSound;

    private Narration narration;

    private AdditionalInfoBoard additionalInfoBoard;
    public Pinboard pinboard;
    private float removingTime = -1f;
    private Animator selectedPenAnim;
    private Vector3 penPos;

    private GameManager gm;

    private AudioManager am;

    private bool onHoldDown = false;

    public DeletionEvent OnPinDeletion;

    private Vector3 originalPosition;

    private Dictionary<Connections, GameObject> foundConnections = new Dictionary<Connections, GameObject>();

    private float rotationOffset = 0;

    [SerializeField] private Transform grabPos;

    private Grabbable grabbedObject;

    [SerializeField] private GameObject trashedPostItPrefab;

    private float timeDragged = 0;

    private Archives archives;

    private PauseMenu pauseMenu;


    private RaycastHit hit;
    private Ray ray;
    private bool hasHitSomething;
    bool requirementMet = true;


    [SerializeField] private Camera threadCamera;
    public void ResetFoundConnections()
    {
        foreach (GameObject v in foundConnections.Values)
        {
            Destroy(v);
        }
        foundConnections.Clear();
    }

    void Start()
    {
        gm = GameManager.instance;
        am = AudioManager.instance;
        pinboard = FindObjectOfType<Pinboard>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        inputOverlay = gameObject.GetComponentInChildren<InputOverlay>();

        computerControls = GameObject.Find("DesktopInterface").GetComponent<ComputerControls>();
        narration = GetComponentInChildren<Narration>();
        additionalInfoBoard = transform.GetChild(0).Find("MoreInfo").GetComponent<AdditionalInfoBoard>();
        gm.OnNewDay.AddListener(ResetFoundConnections);
        gm.OnNewDay.AddListener(ResetPlayer);
        threadCamera.enabled = false;
        pauseMenu = FindObjectOfType<PauseMenu>();
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
                DeleteElement();
            }
        }

        ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        hasHitSomething = Physics.Raycast(ray, out hit);

        #region Handles Interaction

        // if removingTime is the default value it is active and the currentselectedObject shouldn't change
        #region Look at things
        if (hasHitSomething && !onHoldDown && gm.GetGameState() == GameState.Playing)
        {
            print(nameOfThingLookedAt);
            if (Vector3.Distance(hit.collider.gameObject.transform.position, transform.position) <= interactionReach || selectedPinboardElement != null || currentThread != null)
            {
                string tempName = hit.collider.gameObject.name;
                if (tempName == "PinboardModel" || tempName == "pin" || tempName == "DeadZone" || tempName == "Pinboard" || tempName == "pinboardElement(Clone)")
                {
                    if (!threadCamera.enabled)
                        threadCamera.enabled = true;
                }

                if (hit.collider.gameObject.tag == "Grabbable")
                {
                    nameOfThingLookedAt = hit.collider.gameObject.name;
                    inputOverlay.SetIcon("handOpen");
                }
                else if (hit.collider.gameObject.tag == "Interactable")
                {
                    nameOfThingLookedAt = hit.collider.gameObject.name;

                    // if lastSelectedObject the player is grabing a thread and shouldn't be able see anything else
                    if (grabbedObject != null)
                    {
                        if (nameOfThingLookedAt == "PinboardModel" && grabbedObject.GetComponent<TrashedPostIt>() != null)
                        {
                            inputOverlay.SetIcon("pin");
                        }
                    }
                    else if (lastSelectedObject == null)
                    {
                        // handle interaction, while just looking
                        switch (nameOfThingLookedAt)
                        {
                            case "PinboardModel":
                                // interactionReach = 10f;
                                if (selectedPinboardElement == null && currentThread == null)//TODO: add Pen exception
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
                            case "Archive":
                                inputOverlay.SetIcon("inspect");
                                break;
                            case "pin":
                                inputOverlay.SetIcon("trash");
                                break;
                            case "threadCollider":
                                inputOverlay.SetIcon("scissors");
                                break;
                            case "Door":
                            if(!narration.TransitionRunning())
                                inputOverlay.SetIcon("exit");
                                break;
                            case "Pen":
                                inputOverlay.SetIcon("handOpen");
                                break;
                            case "Phone":
                                inputOverlay.SetIcon("inspect");
                                break;
                            case "Radio":
                                inputOverlay.SetIcon("radio");
                                break;
                            case "Block":
                                inputOverlay.SetIcon("handOpen");
                                break;
                            default:
                                inputOverlay.SetIcon("inspect");
                                break;
                        }
                    }
                    if (nameOfThingLookedAt != "pinboardElement(Clone)" && nameOfThingLookedAt != "pin")// currentSelectedObject.name != "pinboardElement(Clone)" && currentSelectedObject.name != "pin"
                    {
                        // undoes highlight and scaling of the pinboardElement  
                        if (currentSelectedObject != null && (currentSelectedObject.name == "pinboardElement(Clone)" || currentSelectedObject.name == "pin"))
                        {
                            if (selectedPinboardElement == null)
                            {
                                // This is so hovering on the pin won't cancel the AdditionalInfoBoard
                                PinboardElement tempPe = currentSelectedObject.GetComponent<PinboardElement>();
                                if (currentSelectedObject.name == "pin")
                                {
                                    tempPe = currentSelectedObject.GetComponentInParent<PinboardElement>();
                                }
                                if (tempPe != null)
                                    tempPe.HighlightElement(false);
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
                if (threadCamera.enabled)
                {
                    threadCamera.enabled = false;
                }
                DeselectPen();
                if (selectedPinboardElement != null)
                {
                    selectedPinboardElement.gameObject.layer = 0;
                    selectedPinboardElement.transform.Find("pin").GetComponent<Collider>().enabled = true;
                    selectedPinboardElement.transform.position = originalPosition;
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
                additionalInfoBoard.CancelPreview();
                inputOverlay.ChangeIconIfDifferent("");
                //interactionReach = 3f; 
            }
            else
            {
                //interactionReach = 3f;
                inputOverlay.ChangeIconIfDifferent("");
            }
        }
        requirementMet = true;
        if (narration.HasRequirement())
        {
            if (nameOfThingLookedAt == "PinboardModel" || nameOfThingLookedAt == "Block")
            {
                narration.CheckIfRequirementMet(Requirement.WalkToBoard);
            }
            else if (nameOfThingLookedAt == "pinboardElement(Clone)")
            {
                if (currentSelectedObject != null && currentSelectedObject.GetComponent<PinboardElement>().GetIfHasInfo())
                {
                    narration.CheckIfRequirementMet(Requirement.LookAtPostIt);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (currentSelectedObject != null && (currentSelectedObject.name == "pinboardElement(Clone)" || currentSelectedObject.name == "pin") && currentThread != null && lastSelectedObject != currentSelectedObject)
                {
                    GameObject postIt = currentSelectedObject;
                    if (currentSelectedObject.name == "pin")
                    {
                        postIt = currentSelectedObject.transform.parent.gameObject;
                    }
                    Person person = postIt.GetComponent<PinboardElement>().GetContent() as Person;
                    if (person == null)
                    {
                        SocialMediaUser user = postIt.GetComponent<PinboardElement>().GetContent() as SocialMediaUser;
                        if (user != null && user.name == "0Imar")
                        {
                            narration.CheckIfRequirementMet(Requirement.ConnectPostITContradiction);
                            narration.CheckIfRequirementMet(Requirement.ConnectPostIT);
                        }
                        else if (!postIt.GetComponent<PinboardElement>().GetIfDeletable())
                        {
                            narration.CheckIfRequirementMet(Requirement.FindSuspect);
                        }
                    }
                    else
                    {
                        if (person.name == "0Imar")
                        {
                            narration.CheckIfRequirementMet(Requirement.ConnectPostIT);
                            narration.CheckIfRequirementMet(Requirement.FindSuspect);
                        }
                        else if (person.name == "0Olga")
                        {
                            narration.CheckIfRequirementMet(Requirement.ConnectPostITContradiction);
                            narration.CheckIfRequirementMet(Requirement.FindSuspect);
                        }
                    }
                }
                else if (currentSelectedObject != null)
                {
                    switch (currentSelectedObject.name)
                    {
                        case "pinboardElement(Clone)":
                            narration.CheckIfRequirementMet(Requirement.MovePostIt);
                            break;
                        case "pin":
                            narration.CheckIfRequirementMet(Requirement.DeletePostIt);
                            break;
                        default:
                            break;
                    }
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (currentSelectedObject.name == "pinboardElement(Clone)" || currentSelectedObject.name == "pin")
                {
                    GameObject postIt = currentSelectedObject;
                    if (currentSelectedObject.name == "pin")
                    {
                        postIt = currentSelectedObject.transform.parent.gameObject;
                    }

                    Person person = postIt.GetComponent<PinboardElement>().GetContent() as Person;
                    if (person != null)
                    {
                        if (person.name == "0Imar")
                        {
                            narration.CheckIfRequirementMet(Requirement.ConnectPostIT, false);
                            narration.CheckIfRequirementMet(Requirement.FindSuspect, false);

                        }
                        else if (person.name == "0Olga")
                        {
                            narration.CheckIfRequirementMet(Requirement.ConnectPostITContradiction, false);
                            narration.CheckIfRequirementMet(Requirement.FindSuspect, false);
                        }
                    }
                    else
                    {
                        SocialMediaUser user = postIt.GetComponent<PinboardElement>().GetContent() as SocialMediaUser;
                        if (user != null)
                        {
                            if (user.name == "0Imar")
                            {
                                narration.CheckIfRequirementMet(Requirement.ConnectPostITContradiction, false);
                                narration.CheckIfRequirementMet(Requirement.ConnectPostIT, false);
                            }
                            else if (!postIt.GetComponent<PinboardElement>().GetIfDeletable())
                            {
                                narration.CheckIfRequirementMet(Requirement.FindSuspect, false);
                            }
                        }
                    }
                }
            }
            requirementMet = narration.GetIfInteractionAllowed();
        }

        #endregion
    }

    private void DeleteElement()
    {
        if (currentSelectedObject != null && currentSelectedObject.transform.parent != null && currentSelectedObject.transform.parent.name == "Connection")
        {
            Destroy(currentSelectedObject.transform.parent.gameObject);
            return;
        }
        PinboardElement pe = currentSelectedObject != null && currentSelectedObject.name == "pin" ? currentSelectedObject.transform.parent.GetComponent<PinboardElement>() : selectedPinboardElement.GetComponent<PinboardElement>();

        if (pe.GetIfDeletable())
        {
            pe.DeleteElement();
            am.PlayAudio(deleteSound);
            //TODO: for now don't spawn any trashed post it if there is no content
            if (pe.GetContent() != null)
            {
                additionalInfoBoard.CancelPreview();
                OnPinDeletion?.Invoke(pe.GetContent());
                //instantiate TrashedPostIT
                if (trashedPostItPrefab != null)
                {
                    GameObject g = Instantiate(trashedPostItPrefab, pe.transform.position + new Vector3(0.1f, 0, 0), pe.transform.rotation);
                    g.GetComponent<TrashedPostIt>().SetContent(pe.GetContent());
                }
            }
        }
        else
        {
            pe.transform.position = originalPosition;
            am.PlayReverseAudio(pickupSound);
            pe.GetComponentInParent<Animator>().SetBool("pinNormal", true);
            pe.gameObject.layer = 0;
            pe.transform.Find("pin").GetComponent<Collider>().enabled = true;
        }

        removingTime = -1;
        inputOverlay.stopHold();
        selectedPinboardElement = null;
    }

    public void ResetCameraRotation(Quaternion rotation, bool instant = false)
    {
        if (instant)
            transform.rotation = rotation;
        else
        {
            if (rotation != Quaternion.identity)
            {
                StartCoroutine(LerpToRotation(rotation));
            }
        }
    }
    void Update()
    {
        if (gm == null)
        {
            gm = GameManager.instance;
        }
        // on e key pressed ExitPC is "E"
        if ((Input.GetButtonDown("ExitPC") || Input.GetKeyDown(KeyCode.S)) && gm.isOccupied())
        {
            if (gm.GetGameState() == GameState.OnPC)
            {
                computerControls.LeaveComputer();
            }
            StartCoroutine(pauseMenu.StartPauseMenuCooldown());
            gm.SetGameState(GameState.Playing);
        }

        if (!gm.isFrozen())
        {
            #region Handles Movment
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            float curSpeedX = canMove ? walkSpeed * Input.GetAxis("Vertical") : 0;
            float curSpeedY = canMove ? walkSpeed * Input.GetAxis("Horizontal") : 0;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            // Play chair rolling sound when moving
            if (curSpeedX != 0 || curSpeedY != 0)
            {
                am.PlayAudioRepeating(chairRollSound, 0.5f, volume: 0.7f);
            }
            else
            {
                am.StopAudioRepeating(chairRollSound, 0.3f);
            }

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

            // if (requirementMet)
            {
                #region Handle left click
                if (Input.GetMouseButtonDown(0))
                {
                    if(hit.collider == null)
                    {
                        return;
                    }
                    //inputOverlay.SetIcon("");
                    if (hit.collider.gameObject.name == "Trash")
                    {
                        SaveManager.instance.DeleteSave();
                        gm.ResetGame();
                        SceneManager.LoadScene(0);
                    }
                    if (hit.collider.gameObject.tag == "Grabbable")
                    {
                        grabbedObject = hit.collider.gameObject.GetComponent<Grabbable>();
                        grabbedObject.Grab(grabPos);
                    }
                    else if (grabbedObject != null)
                    {
                        if (hit.collider.gameObject.name == "PinboardModel" && grabbedObject.GetComponent<TrashedPostIt>() != null)
                        {
                            grabbedObject.GetComponent<TrashedPostIt>().ReAddPostIt();
                            Destroy(grabbedObject.gameObject);
                            grabbedObject = null;
                        }
                        else
                        {
                            grabbedObject.shoot(cameraObject.transform.forward);//Vector3.forward);  
                            grabbedObject = null;
                        }
                    }

                    else if (currentSelectedObject != null)
                    {
                        //Connect threat to pinboardElement
                        if (currentThread != null)
                        {
                            TryToPlaceThread(requirementMet ? currentSelectedObject.name : "false");
                        }
                        // places the pinboardElement back to the pinboard
                        else if (selectedPinboardElement != null)
                        {  // change back to default layer so it can be selected again
                            PlacePinboardElement(nameOfThingLookedAt);
                        }
                        else
                        {
                            // Deselect pen if clicking somewhere, where you can't draw
                            if (selectedPenAnim != null && !(currentSelectedObject.name == "pin" || currentSelectedObject.name == "Pen" || currentSelectedObject.name == "pinboardElement(Clone)"))
                            {
                                DeselectPen();
                            }
                                print(currentSelectedObject.name);
                            switch (currentSelectedObject.name)
                            {
                                case "pinboardElement(Clone)":
                                    if (selectedPenAnim != null)
                                    {
                                        StartCoroutine(PlayPenAnimation("circle"));
                                    }
                                    else
                                    {
                                        timeDragged = Time.time;
                                        originalPosition = currentSelectedObject.transform.position;
                                        inputOverlay.SetIcon("handClosed");
                                        currentSelectedObject.GetComponentInParent<Animator>().SetBool("pinNormal", false);
                                        selectedPinboardElement = currentSelectedObject;
                                        selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(true);
                                        am.PlayAudio(pickupSound);
                                        // change to ignore raycast layer so it won't overflow the pinboard
                                        selectedPinboardElement.gameObject.layer = 2;
                                        selectedPinboardElement.transform.Find("pin").GetComponent<Collider>().enabled = false;
                                    }
                                    break;
                                case "CurvedScreen":
                                    if (requirementMet)
                                    {
                                        gm.SetGameState(GameState.OnPC);
                                        inputOverlay.SetIcon("");
                                        computerControls.ToggleCursor();
                                        am.PlayAudio(pcAccessSound);
                                    }
                                    break;
                                case "threadCollider":
                                    am.PlayAudio(threadCuttingSound);
                                    if (hit.collider.transform.parent.gameObject == pinboard.FlaggedThread)
                                    {
                                        pinboard.FlaggedPersonPin.SetAnnotationType(AnnotationType.None);
                                        pinboard.FlaggedPersonPin = null;
                                        pinboard.FlaggedThread = null;
                                    }
                                    Destroy(hit.collider.transform.parent.gameObject);
                                    break;
                                case "pin":
                                    if (requirementMet)
                                    {
                                        onHoldDown = true;
                                        currentSelectedObject.GetComponentInParent<Animator>().SetTrigger("remove");
                                        am.PlayAudio(pickupSound);
                                        removingTime = 0.5f;
                                        inputOverlay.startHold(removingTime);
                                    }
                                    break;
                                case "Pen":
                                    if (requirementMet)
                                    {
                                        penPos = currentSelectedObject.transform.position;
                                        selectedPenAnim = currentSelectedObject.GetComponentInChildren<Animator>();
                                        selectedPenAnim.transform.parent.gameObject.layer = 2;
                                        selectedPenAnim.SetBool("pickedup", true);
                                    }
                                    break;
                                case "Calendar":
                                    if (requirementMet)
                                    {
                                        inputOverlay.SetIcon("");
                                        gm.InspectObject(hit.collider.transform, new Vector3(0, 0, 1.2f));
                                        // gm.SetGameState(GameState.Inspecting);  
                                    }
                                    break;
                                case "Archive":
                                    if (requirementMet && gm.GetGameState() != GameState.InArchive)
                                    {
                                        archives = currentSelectedObject.transform.gameObject.GetComponent<Archives>();
                                        inputOverlay.SetIcon("");
                                        //  gm.InspectObject(hit.collider.transform.GetChild(0), new Vector3(0, 2f, 2f));
                                        // gm.InspectObject(currentSelectedObject.transform.GetChild(0), new Vector3(0, 2f, 3f), GameState.InArchive);
                                        gm.InspectObject(currentSelectedObject.transform, new Vector3(0, 1f, 2f), GameState.InArchive);
                                        archives.open();
                                        ArchiveManager.Instance.SetCurrentArchive(archives);
                                        // gm.SetGameState(GameState.Inspecting);  
                                    }
                                    break;
                                case "Phone":
                                print(hit.collider.transform.name);
                                    if (hit.collider.transform.GetComponent<Phone>() != null)
                                    {
                                print(currentSelectedObject.name + "  .... " + hit.collider.transform.name);
                                        inputOverlay.SetIcon("");
                                        hit.collider.transform.GetComponent<Phone>().StartCall();
                                    }
                                    break;
                                case "Door":
                                    if (requirementMet)
                                    {
                                        if (gm.GetAnswerCommited() && !FindObjectOfType<Phone>().GetIsRinging())
                                        {
                                            StartCoroutine(EndDay());
                                            // Reset player position
                                        }
                                        else
                                        {
                                            // Todo: implement player feedback
                                            narration.Say("notLeaving");
                                        }
                                    }
                                    break;
                                case "Radio":
                                    if (requirementMet && hit.collider.transform != null && hit.collider.transform.GetComponent<Radio>() != null)
                                    {
                                        hit.collider.transform.GetComponent<Radio>().ChangeChanel();
                                    }
                                    break;
                                case "Block":
                                    narration.Say("blocked");
                                    break;

                                default:
                                    switch (hit.collider.gameObject.name)
                                    {
                                        case "PostItBill": 
                                            inputOverlay.SetIcon("");
                                            gm.InspectObject(hit.collider.transform, new Vector3(0, 1.2f, 0));
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    if (onHoldDown)
                    {
                        onHoldDown = false;
                        inputOverlay.stopHold();
                        removingTime = -1f; 
                        //inputOverlay.SetIcon("");
                    }
                    //Also allow for dragging the pinboardElement
                    else if (selectedPinboardElement != null)
                    {
                        if (Time.time - timeDragged > 0.3f)
                        {
                            PlacePinboardElement(nameOfThingLookedAt);
                          //  inputOverlay.SetIcon("");
                        }
                    }
                    else
                    {
                        //inputOverlay.SetIcon("");
                    }
                }
                #endregion

                if (requirementMet)
                {
                    #region Handle right click
                    if (Input.GetMouseButtonDown(1) && currentSelectedObject != null && (currentSelectedObject.name == "pinboardElement(Clone)" || currentSelectedObject.name == "pin"))
                    {
                        // this allows, the thread to be created while looking at pin
                        if (currentSelectedObject.name == "pin")
                        {
                            currentSelectedObject = currentSelectedObject.transform.parent.gameObject;
                        }
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
                }
                // place the thread after stopping to drag
                if (Input.GetMouseButtonUp(1) && currentThread != null)
                {
                    if (lastSelectedObject.transform != hit.collider.transform && !(lastSelectedObject.transform == hit.collider.transform.parent && hit.collider.gameObject.name == "pin"))
                    {
                        TryToPlaceThread(nameOfThingLookedAt);
                    }
                }
                #endregion
            }
            #region handle selected element movement

            // handle moving pinboardElement position 
            if (selectedPinboardElement != null)
            {
                string targetName = hit.collider.gameObject.name;
                if ((targetName == "PinboardModel" || targetName == "DeadZone") && currentSelectedObject != null && hit.collider.gameObject.tag == "Interactable")
                {
                    selectedPinboardElement.transform.position = new Vector3(selectedPinboardElement.transform.position.x, hit.point.y, hit.point.z);
                }
                /* else if (nameOfThingLookedAt != "pinboardElement(Clone)" && nameOfThingLookedAt != "pin")
                 {
                     onHoldDown = false; 
                     inputOverlay.stopHold();
                     removingTime = -1; // cancel the deletion if in progress
                     selectedPinboardElement.transform.position = originalPosition;
                     PlacePinboardElement();
                 }*/
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

            if (am.IsPlaying(chairRollSound))
                am.StopAudioRepeating(chairRollSound, 0.3f);
        }
    }

    private void PlacePinboardElement(string bgName = "")
    {
        if (selectedPinboardElement == null)
        {
            return;
        }
        if (bgName != "PinboardModel")
        {
            selectedPinboardElement.transform.position = originalPosition;
            onHoldDown = false;
            inputOverlay.stopHold();
            removingTime = -1;
        }
        selectedPinboardElement.gameObject.layer = 0;
        selectedPinboardElement.transform.Find("pin").GetComponent<Collider>().enabled = true;
        selectedPinboardElement.GetComponentInParent<Animator>().SetBool("pinNormal", true);
        selectedPinboardElement.GetComponent<PinboardElement>().setIsMoving(false);
        am.PlayReverseAudio(pickupSound);
        selectedPinboardElement = null;
       // inputOverlay.SetIcon("");
    }

    public void ResetPlayer()
    {
        StartCoroutine(waitForPlayerToBeReady());
    }
    public IEnumerator waitForPlayerToBeReady()
    {
        yield return new WaitForEndOfFrame();
        transform.position = gm.GetStartPosition();
        transform.rotation = gm.GetStartRotation();
        rotationX = 0;
        cameraObject.localRotation = Quaternion.Euler(0, 0, 0);
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
        if (underground == "pinboardElement(Clone)" || underground == "pin")
        {
            if (underground == "pin")
            {
                currentSelectedObject = currentSelectedObject.transform.parent.gameObject;
            }
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
        pinboard.AddConnectionIfExist(currentPE.GetContent(), lastPE.GetContent(), currentThread.transform);

        // the suspicios person post it was connected 
        if (!currentPE.GetIfDeletable() || !lastPE.GetIfDeletable())
        {
            if (currentPE.GetContent() is Person || lastPE.GetContent() is Person)
            {
                if (pinboard.FlaggedPersonPin != null)
                {
                    pinboard.FlaggedPersonPin.GetComponent<PinboardElement>().SetAnnotationType(AnnotationType.None);
                }
                if (pinboard.FlaggedThread != null)
                {
                    Destroy(pinboard.FlaggedThread);
                }
                pinboard.FlaggedThread = currentThread.gameObject;
            }

            if (currentPE.GetContent() is Person)
            {
                gm.checkSuspect(currentPE.GetContent() as Person);
                currentPE.SetAnnotationType(AnnotationType.CaughtSuspect);
                pinboard.FlaggedPersonPin = currentSelectedObject.GetComponent<PinboardElement>();
            }
            else if (lastPE.GetContent() is Person)
            {
                gm.checkSuspect(lastPE.GetContent() as Person);
                lastPE.SetAnnotationType(AnnotationType.CaughtSuspect);
                pinboard.FlaggedPersonPin = lastSelectedObject.GetComponent<PinboardElement>(); ;
            }
        }
    }
    public IEnumerator EndDay()
    {
        am.PlayAudio(doorOpenSound);
        yield return narration.BlackScreenEnumerator(true);
        //yield return new WaitForSeconds(0.5f);
        inputOverlay.SetIcon("");
        gm.setNewDay();
        ResetPlayer();
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

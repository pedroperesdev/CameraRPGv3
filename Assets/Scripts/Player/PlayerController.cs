using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;


/// <summary>
/// Acts as the central point to the user.
/// Reads input and delegates actions to the corresponding script, via event actions (Observer Pattern with PlayerController as the subject) 
/// when other scripts need to react/listen or direct external script calls.
/// Lives inside the player gameobject.
/// </summary>
public class PlayerController : MonoBehaviour
{

    // Player
    // Global player position is known by other scripts
    public static Transform PlayerTransform;
    private CharacterController cc;
    private Camera cam;
    private HUDController hudc;
    private MemoryInventory mi;

    //Enums 
    private enum AnimationStates { None, StartAim, StopAim };

    //Player Anim State
    private AnimationStates currAnimState = AnimationStates.None;
    private Coroutine currAnimCorout;

    private AnimationStates CurrAnimState
    {
        get => currAnimState;
        set
        {
            if(currAnimState == value) return;
            currAnimState = value;
            OnAnimationStateChanged();
        }
    }

    //Player Action States
    bool isAiming { get; set; } = false;
    
    //Player State Getters
    public bool Aiming => isAiming;

    //Actions/Inputs
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction aimAction;
    private InputAction shootAction;
    private InputAction[] selectSlotActions;
    private InputAction scrollSlotAction;
    private InputAction interactAction;


    //Player values
    private float g = -9.8f;
    private float vertVel = 0;
    private float movSpeed = 5f;
    private float lookSensitivity = 8f;
    private string currEnemyName = null;
    private string currTextInfo = null;
    private float minDistCamRay = 1f;
    private float maxDistCamRay = 14f;
    private int numberOfSlots = 6;
    private float maxDistInteract = 2f;

    //Player objects
    [SerializeField] GameObject cameraObj;

    //Local positions
    private Vector3 cameraAimPos = new Vector3(0,0, 0.80f);
    private Vector3 cameraNeutralPos = new Vector3(0.62f,-0.016f,0.93f);

    //Auxiliary
    float elapsed;
    float duration;

    //Player events
    public static event Action<string> OnEnemyAim;
    public static event Action<string> OnHoverInteractable;

    //Info messages
    string hoverItemText = "Press [E] to pick up";

    void Awake()
    {
        //Actions/Inputs
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        interactAction = InputSystem.actions.FindAction("Interact");
        aimAction = InputSystem.actions.FindAction("Aim");
        shootAction = InputSystem.actions.FindAction("Shoot");
        scrollSlotAction = InputSystem.actions.FindAction("ScrollSlot");
        string temp = "SelectSlot";
        selectSlotActions = new InputAction[numberOfSlots];
        for(int i = 0; i < numberOfSlots; i++)
        {
            selectSlotActions[i] = InputSystem.actions.FindAction(temp + (i+1).ToString());
        }


        PlayerTransform = gameObject.transform;
        cc = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        hudc = GetComponent<HUDController>();
        mi = GetComponent<MemoryInventory>();
    }

    void Update()
    {

        Vector3 resultingMov = Vector3.zero;
        //Input
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        
        if(moveInput != Vector2.zero) 
        {
            Vector2 inputNorm = moveInput.normalized;
            resultingMov = cam.transform.forward * inputNorm.y * movSpeed * Time.deltaTime + cam.transform.right * inputNorm.x * movSpeed * Time.deltaTime;
        }
        if(lookInput != Vector2.zero) 
        {
            Vector3 eulerAngle = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerAngle.x - lookInput.y * lookSensitivity * Time.deltaTime, eulerAngle.y + lookInput.x * lookSensitivity * Time.deltaTime, 0f);
        }
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hitInteract, maxDistInteract))
        {
            Debug.DrawLine(transform.position, hitInteract.point, Color.green);
            if(hitInteract.collider.tag == "Memory")
            {
                //Display item message on hover
                if(currTextInfo != hoverItemText)
                {
                    currTextInfo = hoverItemText;
                    OnHoverInteractable?.Invoke(currTextInfo);
                } 
                if (interactAction.WasPressedThisFrame())
                {
                    bool success = mi.TryAddMemory(hitInteract.collider.GetComponent<MemoryWorld>().PickUp());
                    if (!success)
                    {
                        //log error
                    }
                }
            }
            else
            {
                if(currTextInfo != null)
                {
                    currTextInfo = null;
                    OnHoverInteractable?.Invoke(currTextInfo);
                } 
            }
        }
        else
        {
            if(currTextInfo != null)
            {
                currTextInfo = null;
                OnHoverInteractable?.Invoke(currTextInfo);
            } 
        }

        //------------------SLOT CHANGE------------------
        if (!isAiming)
        {            
            //Check if slot active changed via key
            for(int i = 0; i < numberOfSlots; i++)
            {
                if (selectSlotActions[i].WasPressedThisFrame())
                {
                    mi.SetActiveSlot(i);
                    break;
                }
            }
            //Check if slot active changed via mousewheel
            float scrollInput = scrollSlotAction.ReadValue<float>();
            if(scrollInput != 0)
            {
                mi.ScrollSlotChange((int)scrollInput);
            }
        }

        if(aimAction.WasPressedThisFrame())
        {
            CurrAnimState = AnimationStates.StartAim;
            hudc.SetCamHUD();
        }
        if(aimAction.IsPressed())
        {
            isAiming = true;
            //logic like be able to take photo, show camera borders etc, actual camera snap behavior
            if(Physics.Raycast(cameraObj.transform.position + cam.transform.forward*minDistCamRay, cam.transform.forward, out RaycastHit hit, maxDistCamRay))
            {
                if(hit.collider.CompareTag("Enemy"))
                {
                    if(currEnemyName != hit.collider.name)
                    {
                        currEnemyName = hit.collider.name;
                        OnEnemyAim?.Invoke(currEnemyName);
                    }
                    if (shootAction.WasPressedThisFrame())
                    {
                        //Minigame manager
                    }
                }
                else
                {
                    if(currEnemyName != null)
                    {
                        currEnemyName = null;
                        OnEnemyAim?.Invoke(null);
                    }
                }
            }
            else
            {
                if(currEnemyName != null)
                {
                    currEnemyName = null;
                    OnEnemyAim?.Invoke(null);
                }
            }
        }
        if(aimAction.WasReleasedThisFrame())
        {
            CurrAnimState = AnimationStates.StopAim;
            isAiming = false;
            currEnemyName = null;
            OnEnemyAim?.Invoke(null);
            hudc.SetPlayerHUD();
        }
        if(!cc.isGrounded)
        {
            resultingMov.y = vertVel * Time.deltaTime + 0.5f*g* Time.deltaTime * Time.deltaTime;
            vertVel = vertVel + g * Time.deltaTime; 
        }
        else
        {
            resultingMov.y = 0; vertVel = 0;
        }
        cc.Move(resultingMov);
    }

    IEnumerator StartAiming()
    {
        while(elapsed < duration)
        {
            Vector3 tempCamPos = Vector3.Lerp(cameraNeutralPos, cameraAimPos, elapsed/duration);
            cameraObj.transform.localPosition = tempCamPos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    IEnumerator StopAiming()
    {
        while(elapsed < duration)
        {
            Vector3 tempCamPos = Vector3.Lerp(cameraAimPos, cameraNeutralPos, elapsed/duration);
            cameraObj.transform.localPosition = tempCamPos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    void OnAnimationStateChanged()
    {
        //Stop current animation
        if(currAnimCorout != null) StopCoroutine(currAnimCorout);
        elapsed = 0; duration = 0.2f;
        switch (currAnimState)
        {
            case AnimationStates.StartAim:
            {
                currAnimCorout = StartCoroutine(StartAiming());
                break;
            }
            case AnimationStates.StopAim:
            {
                currAnimCorout = StartCoroutine(StopAiming());
                break;
            }
        }
    
    }
}

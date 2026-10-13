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

    [Header("Player's global position")]
    [Tooltip("Used by enemies to check their proximity to player")]
    public static Transform PlayerTransform;

    //Components
    private CharacterController cc;
    private Camera cam;

    //Scripts
    private HUDController hudc;
    private MemoryInventory mi;

    //Enums make it possible to send a state relative no animation or a warning and then hud or another class reacts according to that state
    //Enum for possible animation states
    private enum AnimationStates { None, StartAim, StopAim };

    //Enum for possible interactable hover messages
    public enum PlayerHoverInfo {None, PickUp};

    //Enum for possible warnings that can appear on HUD
    public enum PlayerWarnings {InventoryFull, NoMemoryEquipped};

    //Current animation
    private Coroutine currAnimCorout;
    private AnimationStates currAnimState = AnimationStates.None;

    //Current hover info
    private PlayerHoverInfo currHoverInfo = PlayerHoverInfo.None;

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

    //Player States. Maybe state machine in the future?
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


    [Header("Player values")]
    public float lookSensitivity = 0.08f;
    private float g = -9.8f;
    private float vertVel = 0;
    private float movSpeed = 5f;
    private GameObject currEnemy = null;
    private float minDistCamRay = 1f;
    private float maxDistCamRay = 14f;
    private int numberOfSlots = 6;
    private float maxDistInteract = 2f;
    float cameraTimeUntilClick = 1.5f;

    //Player objects
    [SerializeField] GameObject cameraObj;

    //Local variables/auxiliary
    private Vector3 cameraAimPos = new Vector3(0, 0.96f, 0.80f);
    
    private Vector3 cameraNeutralPos = new Vector3(0.62f, 0.98f, 0.93f);
    float elapsed;
    float duration;
    bool justTriedAimEnemy = false;
    bool canSnap = false;
    float currRingVal = 0f;

    //Player events
    public static event Action<GameObject> OnEnemyAim;
    public static event Action<PlayerHoverInfo> OnHoverInteractable;
    public static event Action<PlayerWarnings> OnPlayerWarning;
    public static event Action<float> OnCameraRingIncrease;
    //delete memory only after copied for the combat manager
    public static event Action OnMemoryCopy;

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
        if(GameManager.I.CurrentMode == GameManager.GameStates.Minigame)
        {
            return;
        }
        
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
            transform.rotation = Quaternion.Euler(eulerAngle.x - lookInput.y * lookSensitivity, eulerAngle.y + lookInput.x * lookSensitivity, 0f);
        }
        CheckForInteraction();
        CheckForSlotChange();
        CheckCameraAim();
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

    void CheckForInteraction()
    {
        if(!isAiming && Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hitInteract, maxDistInteract))
        {
            Debug.DrawLine(transform.position, hitInteract.point, Color.green);
            if(hitInteract.collider.tag == "Memory")
            {
                //Display item message on hover
                if(currHoverInfo != PlayerHoverInfo.PickUp)
                {
                    currHoverInfo = PlayerHoverInfo.PickUp;
                    OnHoverInteractable?.Invoke(currHoverInfo);
                } 
                if (interactAction.WasPressedThisFrame())
                {
                    bool success = mi.TryAddMemory(hitInteract.collider.GetComponent<MemoryWorld>().PickUp());
                    if (success)
                    {
                        hitInteract.collider.GetComponent<MemoryWorld>().Despawn();
                    }
                    else
                    {
                        //We treat error msgs on controller because its central point
                        OnPlayerWarning?.Invoke(PlayerWarnings.InventoryFull);
                    }
                }
            }
            else
            {
                if(currHoverInfo != PlayerHoverInfo.None)
                {
                    currHoverInfo = PlayerHoverInfo.None;
                    OnHoverInteractable?.Invoke(currHoverInfo);
                } 
            }
        }
        else
        {
            if(currHoverInfo != PlayerHoverInfo.None)
            {
                currHoverInfo = PlayerHoverInfo.None;
                OnHoverInteractable?.Invoke(currHoverInfo);
            } 
        }
    }

    void CheckForSlotChange()
    {
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
    }

    void CheckCameraAim()
    {
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
                    justTriedAimEnemy = true;
                    if(currRingVal < 1f)
                    {
                        canSnap = false;
                        currRingVal += Time.deltaTime/cameraTimeUntilClick; //delta x in one frame
                    }
                    else
                    {
                        canSnap = true;
                    }
                    OnCameraRingIncrease?.Invoke(currRingVal);
                    //If aiming at a different enemy
                    if(currEnemy != hit.collider.gameObject)
                    {
                        TryClearCameraRing();
                        currEnemy = hit.collider.gameObject;
                        OnEnemyAim?.Invoke(currEnemy);
                    }
                    if (shootAction.WasPressedThisFrame() && canSnap)
                    {
                        Slot tempCopy;
                        Slot mem = mi.TryUseCurrMemory();
                        if (mem != null)
                        {
                            tempCopy = CreateMemCopy(mem);
                            OnMemoryCopy?.Invoke();
                            currRingVal = 0f;
                            hudc.SetPlayerHUD();
                            CurrAnimState = AnimationStates.None;
                            isAiming = false;
                            //get curr enemy
                            Enemy enemy = hit.collider.GetComponent<Enemy>();
                            StartCoroutine(CombatManager.I.Init(tempCopy, enemy));
                        }
                        else
                        {
                            OnPlayerWarning?.Invoke(PlayerWarnings.NoMemoryEquipped);
                        }
                    }
                }
                else
                {
                    if(currEnemy != null)
                    {
                        TryClearCameraRing();
                        currEnemy = null;
                        OnEnemyAim?.Invoke(null);
                    }
                }
            }
            else
            {
                if(currEnemy != null)
                {
                    TryClearCameraRing();
                    currEnemy = null;
                    OnEnemyAim?.Invoke(null);
                }
            }
        }
        if(aimAction.WasReleasedThisFrame())
        {
            TryClearCameraRing();
            CurrAnimState = AnimationStates.StopAim;
            isAiming = false;
            currEnemy = null;
            OnEnemyAim?.Invoke(null);
            hudc.SetPlayerHUD();
        }
    }

    //By using justTriedAimEnemy boolean, it only clears when needed instead of every frame
    void TryClearCameraRing()
    {
        if (justTriedAimEnemy)
        {
            currRingVal = 0f;
            OnCameraRingIncrease?.Invoke(currRingVal);
            justTriedAimEnemy = false;
        }
    }

    Slot CreateMemCopy(Slot slot)
    {
        Slot temp = new Slot();
        temp.memoryId = slot.memoryId;
        temp.displayName = slot.displayName;
        temp.sprite = slot.sprite;
        temp.uses = slot.uses;
        temp.unlocked = slot.unlocked;
        temp.minigame = slot.minigame;
        return temp;
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
            case AnimationStates.None:
            {
                cameraObj.transform.localPosition = cameraNeutralPos;
                break;
            }
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

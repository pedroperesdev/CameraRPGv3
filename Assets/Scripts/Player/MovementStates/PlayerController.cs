using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Acts as the central point to the user.
/// Reads input and delegates behaviour to the current movement state and camera state (State Pattern).
/// Other scripts react/listen via event actions (Observer Pattern with PlayerController as the subject).
/// Lives inside the player gameobject.
/// </summary>
public class PlayerController : MonoBehaviour
{

    //Player Movement States
    public IdleState Idle {get; private set;}
    public WalkState Walk {get; private set;}
    public RunState Run {get; private set;}
    public CrouchState Crouch {get; private set;}
    public PlayerMovementState CurrentMovementState {get; private set;}

    //Player Camera/Interaction States
    public LoweredCamState Lowered {get; private set;}
    public AimCamState Aim {get; private set;}
    public PlayerCameraState CurrentCameraState {get; private set;}


    [Header("Player's global position")]
    [Tooltip("Used by enemies to check their proximity to player")]
    public static Transform PlayerTransform;

    //Components
    [SerializeField] GameObject playerHead;
    [SerializeField] GameObject cameraGroupParent;
    [SerializeField] GameObject cameraGroupWobble;
    public static GameObject PlayerHead {get; private set;}
    public GameObject CameraGroupParent => cameraGroupParent;
    public GameObject CameraGroupWobble => cameraGroupWobble;
    public CharacterController CC { get; private set; }
    public Camera Cam { get; private set; }

    //Scripts
    private HUDController hudc;
    private MemoryInventory mi;

    //Enums make it possible to send a state relative no animation or a warning and then hud or another class reacts according to that state
    //Enum for possible animation states
    public enum AnimationStates { None, StartAim, StopAim };

    //Enum for possible interactable hover messages
    public enum PlayerHoverInfo { None, PickUp };


    //Enum for possible warnings that can appear on HUD
    public enum PlayerWarnings { InventoryFull, NoMemoryEquipped };

    //Current animation
    private Coroutine currAnimCorout;
    private AnimationStates currAnimState = AnimationStates.None;

    //Current hover info
    private PlayerHoverInfo currHoverInfo = PlayerHoverInfo.None;

    public AnimationStates CurrAnimState
    {
        get => currAnimState;
        set
        {
            if(currAnimState == value) return;
            currAnimState = value;
            OnAnimationStateChanged();
        }
    }

    //Player State Getters
    public bool Aiming => CurrentCameraState == Aim;

    //Actions/Inputs
    private InputAction moveAction;
    private InputAction lookAction;
    public InputAction SprintAction { get; private set; }
    public InputAction CrouchAction { get; private set; }
    public InputAction AimAction { get; private set; }
    public InputAction ShootAction { get; private set; }
    private InputAction[] selectSlotActions;
    private InputAction scrollSlotAction;
    private InputAction interactAction;

    public Vector2 LookInput { get; private set; }
    public Vector2 MoveInput { get; private set; }

    public bool MoveHeld { get; private set; }


    [Header("Player values")]
    public float lookSensitivity = 0.08f;
    public enum HitboxState {Normal, Crouch};
    public float normalHitboxHeight = 3f;
    public float crouchHitboxHeight = 1.5f;

    private float maxCamDelta = 85f;
    private float minCamDelta = -85f;
    private float pitch = 0f;
    private float g = -9.8f;
    private float vertVel = 0;

    //Stamina-related
    private float currStamina;
    private float recoveryLimit = 30f;
    private float maxStamina = 100f;
    private float staminaRdcSpeed = 50f; //How fast stamina is spent
    private float staminaIncSpeed = 25f; //How fast stamina is regained
    bool canSprint = true;
    bool sprintNeedsRelease = false;

    //Camera aim-related
    public float MinDistCamRay { get; private set; } = 1f;
    public float MaxDistCamRay { get; private set; } = 14f;
    public float CameraTimeUntilClick { get; private set; } = 1.5f;

    private int numberOfSlots = 6;
    private float maxDistInteract = 2f;

    //Player objects
    [SerializeField] GameObject cameraObj;
    public GameObject CameraObj => cameraObj;

    //Local variables/auxiliary
    [HideInInspector] public Vector3 ResultingMovement;
    private Vector3 cameraAimPos = new Vector3(0, 0.96f, 0.80f);

    private Vector3 cameraNeutralPos = new Vector3(0.62f, 0.98f, 0.93f);
    float elapsed;
    float duration;
    public float StartingWobblePos { get; private set; }
    //Shared between movement states so the wobble phase is continuous when switching walk/run/crouch
    [HideInInspector] public float WobbleElapsed;


    //Player events
    public static event Action<GameObject> OnEnemyAim;
    public static event Action<float> OnStaminaChange;
    public static event Action<PlayerHoverInfo> OnHoverInteractable;
    public static event Action<PlayerWarnings> OnPlayerWarning;
    public static event Action<float> OnCameraRingIncrease;
    //delete memory only after copied for the combat manager
    public static event Action OnMemoryCopy;

    void Awake()
    {
        PlayerHead = playerHead;

        //Init player states
        Idle = new IdleState(this);
        Walk = new WalkState(this);
        Run = new RunState(this);
        Crouch = new CrouchState(this);
        Lowered = new LoweredCamState(this);
        Aim = new AimCamState(this);

        //Actions/Inputs
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        SprintAction = InputSystem.actions.FindAction("Sprint");
        CrouchAction = InputSystem.actions.FindAction("Crouch");
        interactAction = InputSystem.actions.FindAction("Interact");
        AimAction = InputSystem.actions.FindAction("Aim");
        ShootAction = InputSystem.actions.FindAction("Shoot");
        scrollSlotAction = InputSystem.actions.FindAction("ScrollSlot");

        string temp = "SelectSlot";
        selectSlotActions = new InputAction[numberOfSlots];
        for(int i = 0; i < numberOfSlots; i++)
        {
            selectSlotActions[i] = InputSystem.actions.FindAction(temp + (i+1).ToString());
        }
        PlayerTransform = gameObject.transform;
        CC = GetComponent<CharacterController>();
        Cam = GetComponentInChildren<Camera>();
        if(Cam != null) StartingWobblePos = cameraGroupWobble.transform.localPosition.y;
        hudc = GetComponent<HUDController>();
        mi = GetComponent<MemoryInventory>();

        //Set initial values
        currStamina = maxStamina;
        CurrentMovementState = Idle;
        CurrentCameraState = Lowered;
    }

    void Update()
    {
        if(GameManager.I.CurrentMode == GameManager.GameStates.Minigame)
        {
            return;
        }
        ResultingMovement = Vector3.zero;

        //Input
        LookInput = lookAction.ReadValue<Vector2>();
        MoveInput = moveAction.ReadValue<Vector2>();
        // booleans needed to check vector inputs of aim and move
        MoveHeld = MoveInput != Vector2.zero;
        if(!SprintAction.IsPressed()) sprintNeedsRelease = false;

        if(LookInput != Vector2.zero)
        {
            pitch -= LookInput.y * lookSensitivity;
            pitch = Mathf.Clamp(pitch, minCamDelta, maxCamDelta);

            Vector3 eulerAngle = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(pitch, eulerAngle.y + LookInput.x * lookSensitivity, 0f);
        }

        //Camera first, so entering aim can kick the player out of sprint on the same frame
        CurrentCameraState.Tick();
        CurrentMovementState.UpdateState();

        if(!CC.isGrounded)
        {
            ResultingMovement.y = vertVel * Time.deltaTime + 0.5f*g* Time.deltaTime * Time.deltaTime;
            vertVel = vertVel + g * Time.deltaTime;
        }
        else
        {
            //apply just some negative
            ResultingMovement.y = -0.1f; vertVel = 0;
        }
        CC.Move(ResultingMovement);

    }

    public void ChangeMovementState(PlayerMovementState newState)
    {
        if(newState == CurrentMovementState) return;
        if(CurrentMovementState != null) CurrentMovementState.ExitState();
        CurrentMovementState = newState;
        newState.EnterState();
    }

    public void ChangeCameraState(PlayerCameraState newState)
    {
        if(newState == CurrentCameraState) return;
        if(CurrentCameraState != null) CurrentCameraState.ExitState();
        CurrentCameraState = newState;
        newState.EnterState();
    }

    //Stamina
    public bool CanStartSprint => canSprint && !sprintNeedsRelease && CC.isGrounded && !Aiming;

    public void RegenStamina()
    {
        if(currStamina < maxStamina)
        {
            currStamina += staminaIncSpeed * Time.deltaTime;
            //avoid overflow
            if(currStamina >= maxStamina) currStamina = maxStamina;
            OnStaminaChange?.Invoke(currStamina/maxStamina);
            if(currStamina >= recoveryLimit) canSprint = true;
        }
    }

    //Returns false when stamina ran out
    public bool DrainStamina()
    {
        currStamina -= Time.deltaTime * staminaRdcSpeed;
        if(currStamina < 0) currStamina = 0;
        OnStaminaChange?.Invoke(currStamina/maxStamina);
        if(currStamina == 0)
        {
            EnterRecovery();
            return false;
        }
        return true;
    }

    public void StopSprintAndEnterRecovery()
    {
        if(currStamina < recoveryLimit) {
            EnterRecovery();
        }
    }

    void EnterRecovery()
    {
        canSprint = false;
        sprintNeedsRelease = true;
    }

    public void ChangeHitbox(HitboxState hstate)
    {
        switch (hstate)
        {
            case HitboxState.Normal:
            {
                CC.height = normalHitboxHeight;
                Vector3 hitboxCenter = CC.center;
                hitboxCenter.y = normalHitboxHeight/2;
                CC.center = hitboxCenter;
                Vector3 temp = cameraGroupParent.transform.localPosition;
                temp.y = 0;
                cameraGroupParent.transform.localPosition = temp;
                break;
            }
            case HitboxState.Crouch:
            {
                CC.height = crouchHitboxHeight;
                Vector3 hitboxCenter = CC.center;
                hitboxCenter.y = crouchHitboxHeight/2;
                CC.center = hitboxCenter;
                Vector3 temp = cameraGroupParent.transform.localPosition;
                temp.y = -1.5f;
                cameraGroupParent.transform.localPosition = temp;
                break;
            }
        }
    }

    public void CheckForInteraction()
    {
        if(Physics.Raycast(Cam.transform.position, Cam.transform.forward, out RaycastHit hitInteract, maxDistInteract))
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
                ClearHoverInfo();
            }
        }
        else
        {
            ClearHoverInfo();
        }
    }

    public void ClearHoverInfo()
    {
        if(currHoverInfo != PlayerHoverInfo.None)
        {
            currHoverInfo = PlayerHoverInfo.None;
            OnHoverInteractable?.Invoke(currHoverInfo);
        }
    }

    public void CheckForSlotChange()
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

    //HUD
    public void SetPlayerHUD() => hudc.SetPlayerHUD();
    public void SetCamHUD() => hudc.SetCamHUD();

    //Event invokers, since events can only be invoked from inside this class
    public void InvokeEnemyAim(GameObject enemy) => OnEnemyAim?.Invoke(enemy);
    public void InvokeCameraRingIncrease(float val) => OnCameraRingIncrease?.Invoke(val);

    //Returns true if the photo was taken and combat started
    public bool TryTakePhoto(Enemy enemy)
    {
        Slot mem = mi.TryUseCurrMemory();
        if (mem == null)
        {
            OnPlayerWarning?.Invoke(PlayerWarnings.NoMemoryEquipped);
            return false;
        }
        Slot tempCopy = CreateMemCopy(mem);
        OnMemoryCopy?.Invoke();
        ChangeCameraState(Lowered);
        //Snap camera back instantly instead of animating
        CurrAnimState = AnimationStates.None;
        StartCoroutine(CombatManager.I.Init(tempCopy, enemy));
        return true;
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

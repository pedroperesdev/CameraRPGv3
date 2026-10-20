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
    [SerializeField] GameObject cameraGroupParent;
    [SerializeField] GameObject cameraGroupWobble;
    private CharacterController cc;
    private Camera cam;

    //Scripts
    private HUDController hudc;
    private MemoryInventory mi;

    //Enums make it possible to send a state relative no animation or a warning and then hud or another class reacts according to that state
    //Enum for possible animation states
    private enum AnimationStates { None, StartAim, StopAim };

    //Enum for possible interactable hover messages
    public enum PlayerHoverInfo { None, PickUp };

    //Enum for possible warnings that can appear on HUD
    public enum PlayerWarnings { InventoryFull, NoMemoryEquipped };

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
    bool isSprinting { get; set; } = false;
    
    //Player State Getters
    public bool Aiming => isAiming;

    //Actions/Inputs
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private InputAction aimAction;
    private InputAction shootAction;
    private InputAction[] selectSlotActions;
    private InputAction scrollSlotAction;
    private InputAction interactAction;


    [Header("Player values")]
    public float lookSensitivity = 0.08f;
    private enum hitboxStates {Normal, Crouch};
    public float normalHitboxHeight = 3f;
    public float crouchHitboxHeight = 1.5f;

    private float maxCamDelta = 85f;
    private float minCamDelta = -85f;
    private float pitch = 0f;
    private float g = -9.8f;
    private float vertVel = 0;
    //Speed-related
    private float crouchMovSpeed = 3f;
    private float normalMovSpeed = 5f;
    private float sprintMovSpeed = 8f;
    private float currMovSpeed;
    //Stamina-related
    private float currStamina;
    private float recoveryLimit = 30f;
    private float maxStamina = 100f;
    private float staminaRdcSpeed = 50f; //How fast stamina is spent
    private float staminaIncSpeed = 25f; //How fast stamina is regained
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
    bool canSprint = true;
    bool sprintNeedsRelease = false;
    float currRingVal = 0f;
    float startingWobblePos;
    float wobbleElapsed = 0f;
    float wobbleFreq;
    float wobbleAmp = 0.08f;
    float wobbleFreqWalk = 0.8f;
    float wobbleFreqSprint = 1.5f;
    Coroutine stoppingWobble;
    

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
        //Actions/Inputs
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        crouchAction = InputSystem.actions.FindAction("Crouch");
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
        if(cam != null) startingWobblePos = cameraGroupWobble.transform.localPosition.y;
        hudc = GetComponent<HUDController>();
        mi = GetComponent<MemoryInventory>();

        //Set initial values
        currMovSpeed = normalMovSpeed;
        currStamina = maxStamina;
        wobbleFreq = wobbleFreqWalk;
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
        
        CheckSprint();
        if (!isSprinting)
        {
            RegenStamina();
            CheckCrouch();
        }
        if(moveInput != Vector2.zero) 
        {
            Vector2 inputNorm = moveInput.normalized;
            resultingMov = cam.transform.forward * inputNorm.y * currMovSpeed * Time.deltaTime + cam.transform.right * inputNorm.x * currMovSpeed * Time.deltaTime;
            WobbleCamera();
        }
        if(moveInput == Vector2.zero) {
            stoppingWobble = StartCoroutine(StopWobbleSmoothly());
        }
        if(lookInput != Vector2.zero) 
        {
            pitch -= lookInput.y * lookSensitivity;
            pitch = Mathf.Clamp(pitch, minCamDelta, maxCamDelta);

            Vector3 eulerAngle = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(pitch, eulerAngle.y + lookInput.x * lookSensitivity, 0f);
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
            //apply just some negative
            resultingMov.y = -0.1f; vertVel = 0;
        }
        cc.Move(resultingMov);
    }

    void CheckSprint()
    {
        if (!sprintAction.IsPressed()) sprintNeedsRelease = false;  
        if(!isAiming && cc.isGrounded && sprintAction.IsPressed() && canSprint && !sprintNeedsRelease && !isAiming)
        {
            ChangeHitbox(hitboxStates.Normal);
            currStamina -= Time.deltaTime * staminaRdcSpeed;
            if(currStamina < 0) currStamina = 0;
            OnStaminaChange?.Invoke(currStamina/maxStamina);
            wobbleFreq = wobbleFreqSprint;
            currMovSpeed = sprintMovSpeed;
            isSprinting = true;
            if(currStamina == 0) {
                EnterRecovery();
            }
        }
        else if(sprintAction.WasReleasedThisFrame()) {
            StopSprintAndEnterRecovery();
        }
    }

    void StopSprintAndEnterRecovery()
    {
        if(currStamina < recoveryLimit) {
            EnterRecovery();
        }
        currMovSpeed = normalMovSpeed;
        isSprinting = false;
    }

    void WobbleCamera()
    {
        if(stoppingWobble != null) StopCoroutine(StopWobbleSmoothly());
        wobbleElapsed += Time.deltaTime;
        float offset = Mathf.Sin(wobbleElapsed * wobbleFreq * 2 * Mathf.PI) * wobbleAmp;
        Vector3 newCamPos = new Vector3(cameraGroupWobble.transform.localPosition.x, startingWobblePos + offset, cameraGroupWobble.transform.localPosition.z);
        cameraGroupWobble.transform.localPosition = newCamPos;
    }

    IEnumerator StopWobbleSmoothly()
    {
        wobbleElapsed = 0f;
        while (true)
        {
            float newHeight = Mathf.Lerp(cameraGroupWobble.transform.localPosition.y, startingWobblePos, 0.5f * Time.deltaTime);
            if(Mathf.Abs(newHeight - startingWobblePos) <= 0.01) yield break;
            Vector3 newPos = cameraGroupWobble.transform.localPosition;
            newPos.y = newHeight;
            cameraGroupWobble.transform.localPosition = newPos;
            yield return null;
        }
    }

    void CheckCrouch()
    {
        if(!isAiming && cc.isGrounded && crouchAction.IsPressed())
        {
            currMovSpeed = crouchMovSpeed;
            ChangeHitbox(hitboxStates.Crouch);
        }
        else if(crouchAction.WasReleasedThisFrame()) {
            currMovSpeed = normalMovSpeed;
            ChangeHitbox(hitboxStates.Normal);
        }
    }

    void ChangeHitbox(hitboxStates hstate)
    {
        switch (hstate)
        {
            case hitboxStates.Normal:
            {
                cc.height = normalHitboxHeight;
                Vector3 hitboxCenter = cc.center;
                hitboxCenter.y = normalHitboxHeight/2;
                cc.center = hitboxCenter;
                Vector3 temp = cameraGroupParent.transform.localPosition;
                temp.y = 0;
                cameraGroupParent.transform.localPosition = temp;
                break;
            }
            case hitboxStates.Crouch:
            {
                cc.height = crouchHitboxHeight;
                Vector3 hitboxCenter = cc.center;
                hitboxCenter.y = crouchHitboxHeight/2;
                cc.center = hitboxCenter;
                Vector3 temp = cameraGroupParent.transform.localPosition;
                temp.y = -1.5f;
                cameraGroupParent.transform.localPosition = temp;
                break;
            }
        }
    }

    void RegenStamina()
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

    void EnterRecovery()
    {
        canSprint = false;
        wobbleFreq = wobbleFreqWalk;
        currMovSpeed = normalMovSpeed;
        isSprinting = false;
        sprintNeedsRelease = true;
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
            StopSprintAndEnterRecovery();
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
                        TryClearCameraRing();
                    }
                }
                else
                {
                    if(currEnemy != null)
                    {
                        TryClearCameraRing();
                    }
                }
            }
            else
            {
                if(currEnemy != null)
                {
                    TryClearCameraRing();
                }
            }
        }
        if(aimAction.WasReleasedThisFrame())
        {
            TryClearCameraRing();
            CurrAnimState = AnimationStates.StopAim;
            isAiming = false;
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
            currEnemy = null;
            OnEnemyAim?.Invoke(null);
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

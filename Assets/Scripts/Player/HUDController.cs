using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    //HUD States
    enum HUDStates { None, PlayerHUD, CamHUD }

    //PlayerHUD components
    [SerializeField] GameObject playerHUD;
    [SerializeField] Image staminaBar;
    [SerializeField] GameObject[] slotsUI;
    [SerializeField] TextMeshProUGUI enemyName = null;
    [SerializeField] GameObject crosshair;
    [SerializeField] TextMeshProUGUI infoText = null;
    [SerializeField] TextMeshProUGUI warningText = null;
    [SerializeField] Image cameraRing;
    [SerializeField] GameObject camHUD;
    [SerializeField] CanvasGroup fadeRect;
    //Will be activated and disabled on enemy hover
    [SerializeField] GameObject enemyHUD;
    //Enemy portrait parts
    [SerializeField] RawImage enemyPortrait;
    [SerializeField] Image enemyHP;

    GameObject activeSlot;
    Image[] crosshairParts = new Image[2];
    GameObject lastEnemy = null;
    bool staminaBarHidden = true;
    float currAlpha;
    float targetAlpha;
    float transTime = 0.1f;
    private Slot[] slots;
    private Coroutine showWarningCoroutine = null;
    private bool firstInstance = true;
    HUDStates currHUD;
    Coroutine currHUDTransition = null;
    HUDStates CurrHUD
    {
        get => currHUD;
        set
        {
            if(currHUD == value) return;
            currHUD = value;
            OnHUDChanged();
        }
    }

    void Awake()
    {
        staminaBar.fillAmount = 1;
    }

    void OnEnable()
    {
        MemoryInventory.OnSlotsChanged += RefreshSlots;
        MemoryInventory.OnChangeActiveSlot += SetActiveSlot;
        MemoryInventory.OnMemoryUse += UseMemory;
        PlayerController.OnEnemyAim += CheckCurrentEnemy;
        PlayerController.OnHoverInteractable += SetHoverInfo;
        PlayerController.OnPlayerWarning += ShowPlayerWarning;
        PlayerController.OnCameraRingIncrease += SetCameraRingFill;
        PlayerController.OnStaminaChange += UpdateStaminaBar;
    }

    void OnDisable()
    {
        MemoryInventory.OnSlotsChanged -= RefreshSlots;
        MemoryInventory.OnChangeActiveSlot -= SetActiveSlot;
        MemoryInventory.OnMemoryUse -= UseMemory;
        PlayerController.OnEnemyAim -= CheckCurrentEnemy;
        PlayerController.OnHoverInteractable -= SetHoverInfo;
        PlayerController.OnPlayerWarning -= ShowPlayerWarning;
        PlayerController.OnCameraRingIncrease -= SetCameraRingFill;
        PlayerController.OnStaminaChange -= UpdateStaminaBar;
    }

    void Start()
    {
        crosshairParts[0] = crosshair.transform.GetChild(0).GetComponent<Image>();
        crosshairParts[1] = crosshair.transform.GetChild(1).GetComponent<Image>();
        camHUD.SetActive(false);
        playerHUD.SetActive(true);
    }

    void Update()
    {
        
    }

    void UpdateStaminaBar(float newVal)
    {
        if (staminaBarHidden)
        {
            staminaBar.gameObject.SetActive(true);
            staminaBarHidden = false;
        } 
        staminaBar.fillAmount = newVal;
        if(newVal < 0.3) staminaBar.color = Color.gray; else staminaBar.color = Color.white;
        //Hide if full, avoid per-frame
        if(newVal == 1f) {
            staminaBarHidden = true;
            staminaBar.gameObject.SetActive(false);
        }
    }

    void UseMemory(int newUses)
    {
        activeSlot.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = newUses.ToString();
    }

    void ShowPlayerWarning(PlayerController.PlayerWarnings warning)
    {
        string displayText = null;
        if(showWarningCoroutine != null) StopCoroutine(showWarningCoroutine);
        switch (warning)
        {
            case PlayerController.PlayerWarnings.InventoryFull:
            {
                displayText = "Camera roll is full";
                break;     
            }
            case PlayerController.PlayerWarnings.NoMemoryEquipped:
            {
                displayText = "No memory selected";
                break;     
            }
        }
        showWarningCoroutine = StartCoroutine(ShowWarning(displayText));
    }

    IEnumerator ShowWarning(string displayText)
    {
        warningText.text = displayText;
        yield return new WaitForSeconds(2f);
        warningText.text = null;
    }

    void RefreshSlots(Slot[] emittedSlots)
    {
        slots = emittedSlots;
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].unlocked)
            {
                slotsUI[i].SetActive(true);
                slotsUI[i].transform.GetChild(1).GetComponent<Image>().sprite = slots[i].sprite;
                slotsUI[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = slots[i].uses == 0 ? "" : slots[i].uses.ToString();
                slotsUI[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = slots[i].displayName;
            }
            else slotsUI[i].SetActive(false);
        }
        if (firstInstance)
        {
            SetActiveSlot(0);
            firstInstance = false;
        }
    }

    public void SetCameraRingFill(float newVal)
    {
        cameraRing.fillAmount = newVal;
        //cameraRing.fillAmount += ringFillSpeed * Time.deltaTime;
    }
    
    public void SetCamHUD()
    {
        CurrHUD = HUDStates.CamHUD;
    }

    public void SetPlayerHUD()
    {
        CurrHUD = HUDStates.PlayerHUD;
    }

    public void SetNoHUD()
    {
        CurrHUD = HUDStates.None;
    }

    void CheckCurrentEnemy(GameObject enemy)
    {
        if(enemy != null)
        {
            SetEnemyName(enemy.GetComponent<Enemy>().EnemyName);
            SetEnemyHUD(enemy);
        }
        else
        {
            SetEnemyName(null);
            HideEnemyHUD();
        }
    }
    
    public void SetEnemyName(string name)
    {
        if (name != null)
        {
            crosshairParts[0].color = new Color32(0xAD,0x1e,0x1E,255);
            crosshairParts[1].color = new Color32(0xAD,0x1e,0x1E,255);
        }
        else
        {
            crosshairParts[0].color = new Color(0, 0, 0, 255);
            crosshairParts[1].color = new Color(0, 0, 0, 255);
        }
        enemyName.text = name;
    }

    public void SetEnemyHUD(GameObject enemy)
    {
        if(lastEnemy != null) lastEnemy.GetComponentInChildren<Camera>().targetTexture = null;
        //We need to save a ref to last enemy in order to disable output of its camera to texture
        lastEnemy = enemy;
        enemyHP.fillAmount = lastEnemy.GetComponent<Enemy>().CurrHp/lastEnemy.GetComponent<Enemy>().MaxHp;
        lastEnemy.GetComponentInChildren<Camera>().targetTexture = (RenderTexture)enemyPortrait.texture;
        enemyHUD.SetActive(true);
    }

    public void HideEnemyHUD()
    {
        enemyHUD.SetActive(false);
    }

    void OnHUDChanged()
    {
        if(currHUDTransition != null) StopCoroutine(currHUDTransition);
        switch (currHUD)
        {
            case HUDStates.None:
            {
                ChangeToNoHUD();
                break;
            }
            case HUDStates.CamHUD:
            {
                currHUDTransition = StartCoroutine(ChangeToCamHUD());
                break;
            }
            case HUDStates.PlayerHUD:
            {
                currHUDTransition = StartCoroutine(ChangeToNormalHUD());
                break;
            }
        }
    }



    public void SetActiveSlot(int slot)
    {
        //Can only set a slot that is unlocked as active 
        if (slotsUI[slot].activeSelf)
        {
            if(activeSlot != null)
            {
                activeSlot.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            activeSlot = slotsUI[slot];
            activeSlot.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        }
    }

    void ChangeToNoHUD()
    {
        playerHUD.SetActive(false);
        camHUD.SetActive(false);
    }
    
    IEnumerator ChangeToCamHUD()
    {
        currAlpha = 0f; targetAlpha = 1f;
        while(currAlpha != targetAlpha)
        {
            float alpha = Mathf.MoveTowards(currAlpha, targetAlpha, (1/transTime)*Time.deltaTime);
            fadeRect.alpha = alpha;
            currAlpha = alpha;
            yield return null;
        }
        playerHUD.SetActive(false);
        camHUD.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        currAlpha = 1f; targetAlpha = 0f;
        while(currAlpha != targetAlpha)
        {
            float alpha = Mathf.MoveTowards(currAlpha, targetAlpha, (1/transTime)*Time.deltaTime);
            fadeRect.alpha = alpha;
            currAlpha = alpha;
            yield return null;
        }
    }

    IEnumerator ChangeToNormalHUD()
    {
        currAlpha = 0f; targetAlpha = 1f;
        while(currAlpha != targetAlpha)
        {
            float alpha = Mathf.MoveTowards(currAlpha, targetAlpha, (1/transTime)*Time.deltaTime);
            fadeRect.alpha = alpha;
            currAlpha = alpha;
            yield return null;
        }
        camHUD.SetActive(false);
        playerHUD.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        currAlpha = 1f; targetAlpha = 0f;
        while(currAlpha != targetAlpha)
        {
            float alpha = Mathf.MoveTowards(currAlpha, targetAlpha, (1/transTime)*Time.deltaTime);
            fadeRect.alpha = alpha;
            currAlpha = alpha;
            yield return null;
        }
    }

    //Hover warnings
    void SetHoverInfo(PlayerController.PlayerHoverInfo hoverInfo)
    {
        switch (hoverInfo)
        {
            case PlayerController.PlayerHoverInfo.None:
            {
                infoText.text = "";
                break; 
            }
            case PlayerController.PlayerHoverInfo.PickUp:
            {
                infoText.text = "Press [E] to pick up";
                break; 
            }
        }
    }
}
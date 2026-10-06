using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    enum HUDStates { PlayerHUD, CamHUD }

    //PlayerHUD-related
    [SerializeField] GameObject playerHUD;
    [SerializeField] GameObject[] slotsUI;
    [SerializeField] TextMeshProUGUI enemyName = null;
    [SerializeField] GameObject crosshair;
    [SerializeField] TextMeshProUGUI infoText = null;
    GameObject activeSlot;
    int activeSlotNum;
    Image[] crosshairParts = new Image[2];
    //CamHUD-related
    [SerializeField] GameObject camHUD;
    [SerializeField] CanvasGroup fadeRect;
    float currAlpha;
    float targetAlpha;
    float transTime = 0.1f;
    private Slot[] slots;


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
    }

    void Start()
    {
        crosshairParts[0] = crosshair.transform.GetChild(0).GetComponent<Image>();
        crosshairParts[1] = crosshair.transform.GetChild(1).GetComponent<Image>();
        camHUD.SetActive(false);
        playerHUD.SetActive(true);
        MemoryInventory.OnSlotsChanged += RefreshSlots;
        PlayerController.OnEnemyAim += SetEnemyName;
        PlayerController.OnHoverInteractable += SetHoverInfo;
    }

    void RefreshSlots(Slot[] emittedSlots)
    {
        slots = emittedSlots;
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].unlocked)
            {
                slotsUI[i].SetActive(true);
                if(slots[i].memoryId != 0)
                {
                    slotsUI[i].transform.GetChild(1).GetComponent<Image>().sprite = slots[i].sprite;
                    slotsUI[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = slots[i].uses.ToString();
                    slotsUI[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = slots[i].displayName;
                }
            }
            else slotsUI[i].SetActive(false);
        }
        //Set first slot as active by default
        SetActiveSlot(1);
    }
    
    public void SetCamHUD()
    {
        CurrHUD = HUDStates.CamHUD;
    }
    public void SetPlayerHUD()
    {
        CurrHUD = HUDStates.PlayerHUD;
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

    void OnHUDChanged()
    {
        if(currHUDTransition != null) StopCoroutine(currHUDTransition);
        switch (currHUD)
        {
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

    public void ScrollSlotChange(int scrollStrength)
    {
        if(scrollStrength > 0)
        {
            //activeSlotNum from 1-6, to access slot array subtract 1 because its 0-5
            int desiredSlot = activeSlotNum + 1;
            if(slots.Length == desiredSlot - 1 || !slots[desiredSlot - 1].unlocked) SetActiveSlot(1);
            else SetActiveSlot(desiredSlot);
        }
        else if(scrollStrength < 0)
        {
            int desiredSlot = activeSlotNum - 1;
            if(desiredSlot == 0) SetActiveSlot(MaxSlotUnlocked() + 1);
            else SetActiveSlot(desiredSlot);
        }
    }

    //Returns last unlocked slot from 0-5
    int MaxSlotUnlocked()
    {
        for(int i = slots.Length - 1; i >= 0; i--)
        {
            if(slots[i].unlocked) return i;
        }
        return 0;
    }

    //receive a slot number from 1-6
    public void SetActiveSlot(int slot)
    {
        //Can only set a slot that is unlock as active 
        if (slotsUI[slot - 1].activeSelf)
        {
            if(activeSlot != null)
            {
                activeSlot.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            activeSlotNum = slot;
            activeSlot = slotsUI[slot-1];
            activeSlot.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        }
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
    void SetHoverInfo(string info)
    {
        infoText.text = info;
    }
}
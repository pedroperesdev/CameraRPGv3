using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    enum HUDStates { PlayerHUD, CamHUD }

    //PlayerHUD-related
    MemoryInventory MI;
    [SerializeField] GameObject playerHUD;
    [SerializeField] GameObject[] slotsUI;
    [SerializeField] TextMeshProUGUI enemyName = null;
    [SerializeField] GameObject crosshair;
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
        MI = GetComponent<MemoryInventory>();
    }

    void Start()
    {
        crosshairParts[0] = crosshair.transform.GetChild(0).GetComponent<Image>();
        crosshairParts[1] = crosshair.transform.GetChild(1).GetComponent<Image>();
        camHUD.SetActive(false);
        playerHUD.SetActive(true);
        MemoryInventory.OnSlotsChanged += RefreshSlots;
        PlayerController.OnEnemyAim += SetEnemyName;
    }

    void RefreshSlots(Slot[] emittedSlots)
    {
        slots = emittedSlots;
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].unlocked)
            {
                slotsUI[i].SetActive(true);
            } 
        }
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
}
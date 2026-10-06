using System;
using UnityEngine;

/// <summary>
/// Deals with memory storage logic and delegates changes to HUDController via event actions (hud reacts to inventory changes).
/// Lives inside the player gameobject.
/// </summary>
public class MemoryInventory : MonoBehaviour
{
    public static event Action<Slot[]> OnSlotsChangedMI;
    public static event Action<int> OnChangeActiveSlotMI;
    private Slot[] slots = new Slot[6];
    private int activeSlotNum;


    void Awake()
    {
        activeSlotNum = 0;
        for(int i= 0; i < slots.Length; i++)
        {
            slots[i] = new Slot();
        }
    }

    void OnEnable()
    {
        GameManager.OnSlotsChangedGM += UnlockSlot;
    }

    void OnDisable()
    {
        GameManager.OnSlotsChangedGM -= UnlockSlot;
    }

    void Start()
    {
        int unlocked = GameManager.I.unlockedSlots;
        for(int i = 0; i < slots.Length; i++)
        {
            if(unlocked > 0) slots[i].unlocked = true;
            unlocked--;
            OnSlotsChangedMI?.Invoke(slots);
        }
    }

    /// <summary>
    /// Changes the currently selected (active) slot.
    /// </summary>
    public void SetActiveSlot(int slot)
    {
        activeSlotNum = slot;
        OnChangeActiveSlotMI?.Invoke(activeSlotNum);
    }
    
    /// <summary>
    /// Sets the first locked slot as unlocked.
    /// </summary>
    void UnlockSlot()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].unlocked)
            {
                slots[i].unlocked = true;
                break;
            }
        }
        OnSlotsChangedMI?.Invoke(slots);
    }

    /// <summary>
    /// Returns number of unlocked slots.
    /// </summary>
    int GetNumUnlockedSlots()
    {
        int total = 0;
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].unlocked)
            {
                total++;
            }
        }
        return total;
    }

    /// <summary>
    /// Returns number of free (and unlocked) slots.
    /// </summary>
    int GetNumFreeSlots()
    {
        int free = 0;
        int unlocked = GetNumUnlockedSlots();
        for(int i = 0; i < unlocked; i++)
        {
            if (slots[i].memoryId == 0)
            {
                free++;
            }
        }
        return free;
    }

    /// <summary>
    /// Attempts to add memory to inventory. Returns boolean according to sucess.
    /// </summary>
    public bool TryAddMemory(MemoryData md)
    {
        int free = GetNumFreeSlots();
        int lastUnlocked = GetNumUnlockedSlots();
        if(free > 0)
        {
            for(int i = 0; i < lastUnlocked; i++)
            {
                if(slots[i].memoryId == 0)
                {
                    slots[i].memoryId = md.memoryId;
                    slots[i].displayName = md.displayName;
                    slots[i].sprite = md.sprite;
                    slots[i].uses = md.uses;
                    OnSlotsChangedMI?.Invoke(slots);
                    return true;
                }
            }
        }
        return false;
        // return bool? Error message on player + hover text. update UI. 
    }

    public void ScrollSlotChange(int scrollStrength)
    {
        if(scrollStrength > 0)
        {
            int desiredSlot = activeSlotNum + 1;
            if(desiredSlot == slots.Length || !slots[desiredSlot - 1].unlocked) SetActiveSlot(0);
            else SetActiveSlot(desiredSlot);
        }
        else if(scrollStrength < 0)
        {
            int desiredSlot = activeSlotNum - 1;
            if(desiredSlot == -1) SetActiveSlot(MaxSlotUnlocked());
            else SetActiveSlot(desiredSlot);
        }
        OnChangeActiveSlotMI?.Invoke(activeSlotNum);
    }

    
    int MaxSlotUnlocked()
    {
        for(int i = slots.Length - 1; i >= 0; i--)
        {
            if(slots[i].unlocked) return i;
        }
        return 0;
    }
}

/// <summary>
/// Represents one of the slots inside the inventory.
/// </summary>
[Serializable]
public class Slot
{
    public int memoryId;
    public string displayName;
    public Sprite sprite;
    public int uses;
    public bool unlocked;
}

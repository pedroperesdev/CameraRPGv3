using System;
using UnityEngine;

public class MemoryInventory : MonoBehaviour
{
    public static event Action<Slot[]> OnSlotsChanged;
    public Slot[] slots = new Slot[6];

    void Awake()
    {
    }

    void Start()
    {
        GameManager.OnSlotsChangedGM += UnlockSlot;
        int unlocked = GameManager.I.unlockedSlots;
        for(int i = 0; i < slots.Length; i++)
        {
            if(unlocked > 0) slots[i].unlocked = true;
            unlocked--;
            OnSlotsChanged?.Invoke(slots);
        }
    }

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
        OnSlotsChanged?.Invoke(slots);
    }

    int UnlockedSlots()
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

    int FreeSlots()
    {
        int free = 0;
        int unlocked = UnlockedSlots();
        for(int i = 0; i < unlocked; i++)
        {
            if (slots[i].memoryId == 0)
            {
                free++;
            }
        }
        return free;
    }

    public bool TryAddMemory(MemoryData md)
    {
        int free = FreeSlots();
        int lastUnlocked = UnlockedSlots();
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
                    OnSlotsChanged?.Invoke(slots);
                    return true;
                }
            }
        }
        return false;
        // return bool? Error message on player + hover text. update UI. 
    }
}

[Serializable]
public class Slot
{
    public int memoryId;
    public string displayName;
    public Sprite sprite;
    public int uses;
    public bool unlocked;
}

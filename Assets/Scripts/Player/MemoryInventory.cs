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
}

[Serializable]
public class Slot
{
    public int memoryId;
    public Sprite sprite;
    public int uses = 3;
    public bool unlocked;
}

using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public int unlockedSlots = 0;
    public int initialSlots;
    public static event Action OnSlotsChangedGM;
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        I = this;
        //Add initial slots
        for(int i = 0; i < initialSlots; i++)
        {
            AddSlot();
        }
    }

    void AddSlot()
    {
        unlockedSlots++;
        OnSlotsChangedGM?.Invoke();
    }
}

//TASKS
//combat system: raycast camera mostra nome inimigo na lente, e muda pra vermelho
//como guardar melhor este game data?
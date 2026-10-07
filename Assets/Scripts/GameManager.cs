using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public GameStates CurrentMode = GameStates.Overworld;
    public static event Action OnSlotsChanged;
    public int unlockedSlots {get; private set;} = 0;
    public enum GameStates {Overworld, Minigame}

    //Auxiliary
    private int initialSlots = 4;
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
        OnSlotsChanged?.Invoke();
    }

    public void ChangeMode(GameStates state)
    {
        CurrentMode = state;
        switch (state)
        {
            case GameStates.Minigame:
            {
                Time.timeScale = 0f;
                break;
            }
            case GameStates.Overworld:
            {
                Time.timeScale = 1f;
                break;
            }
        }
    }
}

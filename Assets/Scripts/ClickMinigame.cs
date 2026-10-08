using UnityEngine;
using UnityEngine.InputSystem;

public class ClickMinigame : Minigame
{
    private float currClicks = 0f;
    private float targetClicks = 20f;
    private InputAction clickAction;

    void Awake()
    {
        clickAction = InputSystem.actions.FindAction("Shoot");
    }

    void Update()
    {
        if (clickAction.WasPressedThisFrame() && (currClicks < targetClicks))
        {
            Debug.Log("clicked!");
            currClicks += 1;
        }
        if(currClicks >= targetClicks)
        {
            Debug.Log("completed!");
            //emit event to end early?
        }
    }

    public override void Begin()
    {
        
    }

    public override void End()
    {
        
    }
}
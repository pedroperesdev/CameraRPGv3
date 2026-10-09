using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ClickMinigame : Minigame
{
    [SerializeField] Image pink;
    private float currClicks = 0f;
    private float targetClicks = 20f;
    private InputAction clickAction;

    void Awake()
    {
        pink.fillAmount = 0;
        clickAction = InputSystem.actions.FindAction("Shoot");
    }

    void Update()
    {
        if (clickAction.WasPressedThisFrame() && (currClicks < targetClicks))
        {
            pink.fillAmount += 1/20f;
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
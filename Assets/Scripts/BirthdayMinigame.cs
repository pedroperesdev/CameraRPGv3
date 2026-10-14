using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BirthdayMinigame : Minigame
{
    private InputAction clickAction;
    private bool canClick = false;
    private int score = 0;
    private float dmgPerHit = 10;

    void Awake()
    {
        clickAction = InputSystem.actions.FindAction("Shoot");
    }

    void Update()
    {
        if(canClick && clickAction.WasPressedThisFrame())
        {
            score += 1;
            canClick = false;
        }
        else if (clickAction.WasPressedThisFrame())
        {
            canClick = false;
        }

    }

    public override void Begin()
    {
        StartCoroutine(ListenAndBlow());
    }

    IEnumerator ListenAndBlow()
    {
        yield return new WaitForSecondsRealtime(3f);
        canClick = true;
        Debug.Log("now");
        yield return new WaitForSecondsRealtime(1f);
        canClick = false;
        yield return new WaitForSecondsRealtime(1f);
    }

    public override float End()
    {
        return ScoreToDmg();
    }

    protected override float ScoreToDmg()
    {
        //in this minigame its just success or fail so either 0 or full damage
        return score * dmgPerHit;
    }
}
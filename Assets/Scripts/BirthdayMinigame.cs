using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BirthdayMinigame : Minigame
{
    //The elements of the minigame
    [SerializeField] GameObject background;
    [SerializeField] GameObject people;
    [SerializeField] GameObject table;
    [SerializeField] GameObject light;
    [SerializeField] GameObject cake;
    [SerializeField] GameObject shocked;
    [SerializeField] GameObject blowText;

    //Animation
    [SerializeField] Animator cakeAnim;

    //Audio
    [SerializeField] AudioClip birthdayStart;
    [SerializeField] AudioClip birthdaySuccess;
    [SerializeField] AudioClip blowCandle;
    [SerializeField] AudioClip birthdayFail;
    private float birthdayStartDuration = 3.72f;
    private float birthdaySuccessDuration = 4.44f;
    private float birthdayFailDuration = 3f;


    //Minigame specific
    private const string msg = "Blow the candles at the right time";
    private const float duration = 9.16f;


    //Auxiliary
    private InputAction clickAction;
    private bool canClick = false;
    private bool success = false;
    private float dmgPerHit = 10;
    private Vector2 peopleInitialPos;
    private float elapsed = 0f;
    private float shakeAmplitude = 2f;
    private float shakeSpeed = 10f;


    void Awake()
    {
        clickAction = InputSystem.actions.FindAction("Shoot");
        peopleInitialPos = people.GetComponent<RectTransform>().anchoredPosition;
        //Set parameters for this minigame
        TutorialMsg = msg;
        Duration = duration;
    }

    void Update()
    {
        if(canClick && clickAction.WasPressedThisFrame())
        {
            score += 1;
            success = true;
            canClick = false;
            BlowCandles();
        }
        else if(clickAction.WasPressedThisFrame())
        {

            canClick = false;
        }

    }

    void BlowCandles()
    {
        cake.GetComponent<Animator>().SetTrigger("blow");
        light.SetActive(false);
        AudioManager.I.PlaySound(blowCandle);
    }

    public override void Begin()
    {
        StartCoroutine(ListenAndBlow());
    }

    IEnumerator ListenAndBlow()
    {
        Coroutine shaking = StartCoroutine(ShakePeople());
        AudioManager.I.PlaySound(birthdayStart);
        yield return new WaitForSecondsRealtime(birthdayStartDuration);

        StopCoroutine(shaking);
        people.GetComponent<RectTransform>().anchoredPosition = peopleInitialPos;
        blowText.SetActive(true);
        canClick = true;
        yield return new WaitForSecondsRealtime(1f);

        blowText.SetActive(false);
        canClick = false;
        if (success)
        {
            AudioManager.I.PlaySound(birthdaySuccess);
            yield return new WaitForSecondsRealtime(birthdaySuccessDuration);
        }
        else
        {
            //AudioManager.I.PlaySound(birthdayFail);
            shocked.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
        }
    }

    IEnumerator ShakePeople()
    {
        while (true)
        {
            elapsed += Time.unscaledDeltaTime;
            float offset = Mathf.Sin(elapsed * shakeSpeed) * shakeAmplitude;
            people.GetComponent<RectTransform>().anchoredPosition = peopleInitialPos + new Vector2(0, offset);
            yield return null;  
        }
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
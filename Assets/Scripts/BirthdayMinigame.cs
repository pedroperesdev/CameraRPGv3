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


    //Minigame specific
    private const string msg = "Blow the candles at the right time";
    private const float duration = 9f;
    private const float successWindow = 0.5f;


    //Auxiliary
    private InputAction clickAction;
    private bool insideClickWindow = false;
    private bool success = false;
    private bool spentChance = false;
    private float dmgPerHit = 10;
    private Vector2 peopleInitialPos;
    private float elapsed = 0f;
    private float shakeAmplitude = 2f;
    private float shakeSpeed = 10f;
    Coroutine shaking;


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
        if(!insideClickWindow && clickAction.WasPressedThisFrame() && !spentChance)
        {
            spentChance = true;
            PlayFailure();
            BlowCandles();
            EndEarly();
        }
        else if(insideClickWindow && clickAction.WasPressedThisFrame() && !spentChance)
        {
            score += 1;
            success = true;
            spentChance = true;
            BlowCandles();
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
        shaking = StartCoroutine(ShakePeople());
        AudioManager.I.PlaySound(birthdayStart);
        yield return new WaitForSecondsRealtime(birthdayStartDuration);

        StopCoroutine(shaking);
        people.GetComponent<RectTransform>().anchoredPosition = peopleInitialPos;
        blowText.SetActive(true);
        insideClickWindow = true;
        yield return new WaitForSecondsRealtime(successWindow);
        blowText.SetActive(false);
        insideClickWindow = false;
        if (success)
        {
            PlaySuccess();
        }
        else
        {
            if(!spentChance) {
                spentChance = true;
                PlayFailure();
            }
        }
    }

    void PlaySuccess()
    {
        AudioManager.I.PlaySound(birthdaySuccess);
        StartCoroutine(ShakePeople());
    }

    void PlayFailure()
    {
        StopCoroutine(shaking);
        AudioManager.I.StopSfxSounds();
        AudioManager.I.PlaySound(birthdayFail);
        shocked.SetActive(true);
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
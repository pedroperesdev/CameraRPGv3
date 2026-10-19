using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    [SerializeField] GameObject background;
    [SerializeField] GameObject minigameFrame;
    [SerializeField] Image minigameArea;
    [SerializeField] GameObject introText;
    [SerializeField] GameObject introBackground;
    public static CombatManager I;
    //Attributes
    float minigameDuration;
    float tutorialMsgDuration = 2f;

    //Auxiliary
    enum Adjust { Increase, Decrease }
    Minigame currMinigame;
    Slot currMem;
    Enemy currEnemy;
    float decreaseSpeed = 1f;
    float increaseSpeed = 0.75f;
    Coroutine MinigameExecCorout;

    //Assets to be used
    Sprite memImage;

    void Awake()
    {
        I = this;
        Color c = minigameArea.GetComponent<Image>().color;
        c.a = 0f;
        minigameArea.GetComponent<Image>().color = c;
    }

    void Update()
    {

    }

    public IEnumerator Init(Slot mem, Enemy enemy)
    {
        currMem = mem;
        currEnemy = enemy;

        GameManager.I.ChangeMode(GameManager.GameStates.Minigame);
        background.SetActive(true);
        minigameFrame.SetActive(true);

        //clear last values
        minigameArea.sprite = null;

        //load beforehand
        memImage = mem.sprite;
        yield return new WaitForSecondsRealtime(0.5f);

        yield return StartCoroutine(DoIntroAnimation());

        MinigameExecCorout = StartCoroutine(ExecuteMinigame());
    }

    IEnumerator ChangeAlphaImg(Graphic gr, Adjust adj)
    {
        Color currColor = gr.color;
        if(adj == Adjust.Decrease)
        {
            currColor.a = 1f;
            gr.color = currColor;
            yield return new WaitForSecondsRealtime(1f);
            while (currColor.a > 0)
            {
                currColor.a -= decreaseSpeed * Time.unscaledDeltaTime;
                gr.color = currColor;
                yield return null;
            }
        }
        else if(adj == Adjust.Increase)
        {
            currColor.a = 0f;
            gr.color = currColor;
            while (currColor.a < 1)
            {
                currColor.a += increaseSpeed * Time.unscaledDeltaTime;
                gr.color = currColor;
                yield return null;
            }
        }
        yield break;
    }

    IEnumerator DoIntroAnimation()
    {
        if (memImage != null)
        {
            minigameArea.sprite = memImage;
        }
        yield return StartCoroutine(ChangeAlphaImg(minigameArea, Adjust.Increase));
        yield return new WaitForSecondsRealtime(0.5f);
    }

    IEnumerator ExecuteMinigame()
    {
        currMinigame = Instantiate(currMem.minigame, minigameArea.transform);
        currMinigame.OnEndEarly += () => StartCoroutine(StopCoroutineAndEndEarly());
        //after loading minigame, show intro text (tutorial)
        yield return StartCoroutine(ShowIntroText());
        currMinigame.Begin();
        yield return new WaitForSecondsRealtime(currMinigame.Duration);
        float dmg = currMinigame.End();
        Destroy(currMinigame.gameObject);
        End(dmg);
    }


    IEnumerator StopCoroutineAndEndEarly()
    {
        yield return new WaitForSecondsRealtime(2f);
        if(MinigameExecCorout != null) StopCoroutine(MinigameExecCorout);
        float dmg = currMinigame.End();
        Destroy(currMinigame.gameObject);
        End(dmg);
        currMinigame.OnEndEarly -= () => StartCoroutine(StopCoroutineAndEndEarly());
    }

    IEnumerator ShowIntroText()
    {
        introBackground.SetActive(true);
        introText.SetActive(true);
        introText.GetComponent<TextMeshProUGUI>().text = currMinigame.TutorialMsg;
        yield return new WaitForSecondsRealtime(tutorialMsgDuration);
        introText.GetComponent<TextMeshProUGUI>().text = null;
        introText.SetActive(false);
        introBackground.SetActive(false);
    }

    void End(float dmg)
    {
        AudioManager.I.StopSfxSounds();
        background.SetActive(false);
        minigameFrame.SetActive(false);
        GameManager.I.ChangeMode(GameManager.GameStates.Overworld);

        ApplyDmg(dmg);
    }

    void ApplyDmg(float dmg)
    {
        currEnemy.TakeDamage(dmg);
    }


}

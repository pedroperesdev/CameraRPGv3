using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CombatManager : MonoBehaviour
{
    [SerializeField] GameObject background;
    [SerializeField] GameObject minigameFrame;
    [SerializeField] Image minigameArea;
    public static CombatManager I;
    //Attributes
    float minigameDuration = 5f;

    //Auxiliary
    enum Adjust { Increase, Decrease }
    Minigame currMinigame;
    Slot currMem;
    Enemy currEnemy;
    float decreaseSpeed = 1f;
    float increaseSpeed = 0.75f;

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
        //before the image appear animation, alpha will be 0 first time, so we need to set to 1
        Color c = minigameArea.color;
        c.a = 1f;
        minigameArea.color = c;

        //load beforehand
        memImage = Resources.Load<Sprite>("MemoryImages/"+currMem.displayName);
        yield return new WaitForSecondsRealtime(0.5f);

        yield return StartCoroutine(DoIntroAnimation());
        StartCoroutine(ExecuteMinigame());
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
        currMinigame.Begin();
        yield return new WaitForSecondsRealtime(minigameDuration);
        float dmg = currMinigame.End();
        Destroy(currMinigame.gameObject);
        End(dmg);
    }

    void End(float dmg)
    {
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

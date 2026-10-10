using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CombatManager : MonoBehaviour
{
    [SerializeField] GameObject background;
    [SerializeField] GameObject minigameFrame;
    [SerializeField] Image minigameArea;
    [SerializeField] GameObject intro;
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
    VideoClip clip;


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

        //before intro anim, clear last values
        intro.GetComponent<VideoPlayer>().clip = null;
        minigameArea.sprite = null;
        //before the image appear animation, alpha will be 0 fist time, so we need to set to 1
        Color c = minigameArea.color;
        c.a = 1f;
        minigameArea.color = c;

        //load beforehand
        memImage = Resources.Load<Sprite>("MemoryImages/"+currMem.displayName);
        clip = Resources.Load<VideoClip>("MemoryIntros/"+currMem.displayName);
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

        if (clip != null)
        {
            intro.GetComponent<VideoPlayer>().clip = clip;
            intro.GetComponent<VideoPlayer>().Play();
        }
        minigameArea.sprite = null;
        yield return StartCoroutine(ChangeAlphaImg(intro.GetComponent<RawImage>(), Adjust.Decrease));
    }

    IEnumerator ExecuteMinigame()
    {
        currMinigame = Instantiate(currMem.minigame, minigameArea.transform);
        currMinigame.Begin();
        yield return new WaitForSecondsRealtime(minigameDuration);
        currMinigame.End();
        Destroy(currMinigame.gameObject);
        End();
    }

    void End()
    {
        background.SetActive(false);
        minigameFrame.SetActive(false);
        GameManager.I.ChangeMode(GameManager.GameStates.Overworld);

        Color currColor = intro.GetComponent<RawImage>().color;
        currColor.a = 0f;
        intro.GetComponent<RawImage>().color = currColor;
        ApplyDmg();
    }

    void ApplyDmg()
    {
        currEnemy.TakeDamage(10f);
    }


}

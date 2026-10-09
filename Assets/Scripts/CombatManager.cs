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
    Minigame currMinigame;
    Slot currMem;
    bool introAlphaDecrease = false;
    bool introAlphaIncrease = false;
    float decreaseSpeed = 20f;

    void Awake()
    {
        I = this;
        Color c = minigameArea.GetComponent<Image>().color;
        c.a = 0f;
        minigameArea.GetComponent<Image>().color = c;
    }

    void Update()
    {
        IntroAlphaIncrease();
        IntroAlphaDecrease();
    }

    public IEnumerator Init(Slot mem)
    {
        currMem = mem;
        GameManager.I.ChangeMode(GameManager.GameStates.Minigame);
        background.SetActive(true);
        minigameFrame.SetActive(true);
        yield return StartCoroutine(DoIntroAnimation());
        StartCoroutine(ExecuteMinigame());
    }

    IEnumerator DoIntroAnimation()
    {
        Sprite memImage = Resources.Load<Sprite>("MemoryImages/"+currMem.displayName);
        if (memImage != null)
        {
            minigameArea.sprite = memImage;
        }
        introAlphaIncrease = true;
        yield return new WaitForSecondsRealtime(0.5f);

        minigameArea.sprite = null;

        VideoClip clip = Resources.Load<VideoClip>("MemoryIntros/"+currMem.displayName);
        if (clip != null)
        {
            intro.GetComponent<VideoPlayer>().clip = clip;
            intro.GetComponent<VideoPlayer>().Play();
        }
        yield return new WaitForSecondsRealtime(1.5f);
        introAlphaIncrease = false;
        introAlphaDecrease = true;
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

    void IntroAlphaDecrease()
    {
        if (introAlphaDecrease)
        {
            Color currColor = intro.GetComponent<RawImage>().color;
            if(currColor.a < 0)
            {
                introAlphaDecrease = false;
                return;
            }
            currColor.a -= decreaseSpeed * Time.unscaledDeltaTime;
            intro.GetComponent<RawImage>().color = currColor;
        }
    }

    void IntroAlphaIncrease()
    {
        if (introAlphaIncrease)
        {
            Color currColor = minigameArea.GetComponent<Image>().color;
            if(currColor.a > 1)
            {
                introAlphaIncrease = false;
                return;
            }
            currColor.a += decreaseSpeed * Time.unscaledDeltaTime;
            minigameArea.GetComponent<Image>().color = currColor;
        }
    }

    public void End()
    {
        background.SetActive(false);
        minigameFrame.SetActive(false);
        GameManager.I.ChangeMode(GameManager.GameStates.Overworld);

        Color currColor = intro.GetComponent<RawImage>().color;
        currColor.a = 0f;
        intro.GetComponent<RawImage>().color = currColor;
    }


}

using System.Collections;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    [SerializeField] GameObject background;
    [SerializeField] GameObject minigameFrame;
    [SerializeField] Image minigameArea;
    public static CombatManager I;
    //Attributes
    float minigameDuration = 5f;

    //Auxiliary
    Minigame currMinigame;
    Slot currMem;

    void Awake()
    {
        I = this;
    }

    void Update()
    {
        
    }

    public void Init(Slot mem)
    {
        currMem = mem;
        GameManager.I.ChangeMode(GameManager.GameStates.Minigame);
        background.SetActive(true);
        minigameFrame.SetActive(true);
        StartCoroutine(ExecuteMinigame());
    }

    IEnumerator ExecuteMinigame()
    {
        currMinigame = Instantiate(currMem.minigame, minigameArea.transform);
        currMinigame.Begin();
        yield return new WaitForSecondsRealtime(minigameDuration);
        currMinigame.End();
        Destroy(currMinigame);
        End();
    }

    public void End()
    {
        background.SetActive(false);
        minigameFrame.SetActive(false);
        GameManager.I.ChangeMode(GameManager.GameStates.Overworld);
    }


}

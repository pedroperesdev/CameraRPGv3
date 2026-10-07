using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager I;

    //Auxiliary
    void Awake()
    {
        I = this;
    }

    public void Init()
    {
        GameManager.I.ChangeMode(GameManager.GameStates.Minigame);
    }

}

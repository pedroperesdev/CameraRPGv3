using UnityEngine;
public abstract class Minigame : MonoBehaviour
{
    public string TutorialMsg {get; protected set;}
    public float Duration {get; protected set;}
    protected int score = 0;
    public abstract void Begin();
    public abstract float End();
    protected abstract float ScoreToDmg();
}
using System;
using UnityEngine;
public abstract class Minigame : MonoBehaviour
{
    public string TutorialMsg {get; protected set;}
    public float Duration {get; protected set;}
    public event Action OnEndEarly;
    protected int score = 0;
    public abstract void Begin();
    public abstract float End();
    protected void EndEarly()
    {
        OnEndEarly?.Invoke();
    }
    protected abstract float ScoreToDmg();
}
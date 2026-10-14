using UnityEngine;
public abstract class Minigame : MonoBehaviour
{
    public abstract void Begin();
    public abstract float End();
    protected abstract float ScoreToDmg();
}
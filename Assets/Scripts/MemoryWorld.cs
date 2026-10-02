using UnityEngine;

[CreateAssetMenu(fileName = "MemoryWorld", menuName = "Scriptable Objects/MemoryWorld")]
public class MemoryWorld : ScriptableObject
{
    public int memoryId;
    public string displayName;
    public Sprite sprite;
    public int uses = 3;
    //public Minigame minigame;
}

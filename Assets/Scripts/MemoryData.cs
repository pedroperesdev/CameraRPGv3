using UnityEngine;

[CreateAssetMenu(fileName = "MemoryData", menuName = "Scriptable Objects/MemoryData")]
public class MemoryData : ScriptableObject
{
    public int memoryId;
    public string displayName;
    public Sprite sprite;
    public int uses;
    //public Minigame minigame;
}

using UnityEngine;

public class MemoryWorld : MonoBehaviour
{
    [SerializeField] MemoryData memoryData; 
    float rotSpeed = 40f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, rotSpeed * Time.deltaTime, 0f);
    }

    public MemoryData PickUp()
    {
        return memoryData;
    }

    public void Despawn()
    {
        tag = "Untagged";
        Destroy(gameObject, 1f);
    }
}

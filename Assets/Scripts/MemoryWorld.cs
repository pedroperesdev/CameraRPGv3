using UnityEngine;

public class MemoryWorld : MonoBehaviour
{
    [SerializeField] MemoryData memoryData; 
    float rotSpeed = 40f;
    bool pickedUp = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, rotSpeed * Time.deltaTime, 0f);
        if(pickedUp) Destroy(gameObject, 1f);
    }

    public MemoryData PickUp()
    {
        tag = "Untagged";
        pickedUp = true;
        return memoryData;
    }
}

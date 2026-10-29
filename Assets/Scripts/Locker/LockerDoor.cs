using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LockerDoor : MonoBehaviour, IOpenable
{
    [SerializeField] Transform hinge;
    public enum DoorState {Closed, Open}
    public DoorState CurrState {get; private set;}
    private float currentAngle = 0f;
    private float maxRotation = 90f;
    private float openSpeed = 4f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        CurrState = DoorState.Closed;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeOpenState()
    {
        //cooldown
        if(hinge != null)
        {
            if(CurrState == DoorState.Closed) StartCoroutine(StartOpening());
            else if(CurrState == DoorState.Open) StartCoroutine(StartClosing());
        } 
    }

    IEnumerator StartOpening()
    {
        gameObject.tag = "Untagged";
        while(currentAngle <= maxRotation)
        {
            currentAngle += maxRotation * Time.deltaTime * openSpeed;
            hinge.transform.Rotate(new Vector3(0f, -maxRotation * Time.deltaTime * openSpeed, 0f));
            yield return null;
        }
        gameObject.tag = "Container";
        currentAngle = 0f;
        CurrState = DoorState.Open;
    }
    IEnumerator StartClosing()
    {
        gameObject.tag = "Untagged";
        while(currentAngle <= maxRotation)
        {
            currentAngle += maxRotation * Time.deltaTime * openSpeed;
            hinge.transform.Rotate(new Vector3(0f, maxRotation * Time.deltaTime * openSpeed, 0f));
            yield return null;
        }
        gameObject.tag = "Container";
        currentAngle = 0f;
        CurrState = DoorState.Closed;
    }
}

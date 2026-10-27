using System;
using System.Collections;
using UnityEngine;

public class Tall : Enemy
{
    // Components
    [SerializeField] Transform head; // Where raycast checks player 

    // Patrol route points
    [SerializeField] private Transform patrolStart;
    [SerializeField] private Transform patrolEnd;
    public Transform PatrolStart => patrolStart;
    public Transform PatrolEnd => patrolEnd;
    public TallState Alerted {get; private set;}
    public TallState Chase {get; private set;}
    public TallState Confused {get; private set;}
    public TallState Patrol {get; private set;}
    public TallState CurrentState {get; private set;}
    private Rigidbody rb;

    private float alertDistance = 15f;
    private float alertAngleTolerance = 65f;
    private float jumpForce = 100f;

    protected override void Awake()
    {
        base.Awake();
        Alerted = new AlertedState(this);
        Chase = new ChaseState(this);
        Confused = new ConfusedState(this);
        Patrol = new PatrolState(this);
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        ChangeState(Patrol);
    }

    public void ChangeState(TallState state)
    {
        if(CurrentState != null) CurrentState.ExitState();
        CurrentState = state;
        CurrentState.EnterState(); 
    }

    //Checks distance to player + direct line of sight
    public bool IsAlerted()
    {
        //min dist + angle + raycast to check objects in between
        if(IsCloseToPlayer() && IsPlayerInFOV() && IsPlayerInLOS())
        {
            return true;
        }
        return false;
    }

    void Update()
    {
        CurrentState.UpdateState();
    }

    public void JumpOnce()
    {
        if(rb != null) rb.AddForce(Vector3.up * jumpForce);
    }

    Vector3 VectorToPlayer()
    {
        return PlayerController.PlayerTransform.position - head.transform.position;
    }

    bool IsCloseToPlayer()
    {
        return Vector3.Distance(PlayerController.PlayerTransform.position, transform.position) < alertDistance;
    }

    bool IsPlayerInFOV()
    {
        return Vector3.Angle(transform.forward, VectorToPlayer()) < alertAngleTolerance;
    }

    bool IsPlayerInLOS()
    {
        return Physics.Raycast(head.position, VectorToPlayer(), out RaycastHit hit, alertDistance) && hit.collider.CompareTag("Player");
    }


    public void LookAtPlayer()
    {
        Vector3 temp = VectorToPlayer();
        temp.y = 0;
        transform.forward = temp;
    }
}
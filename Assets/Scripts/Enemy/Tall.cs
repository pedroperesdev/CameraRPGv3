using System;
using System.Collections;
using UnityEngine;

public class Tall : Enemy
{
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

    private float alertDistance = 10f;
    private float alertAngleTolerance = 45f;
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
        if(IsCloseToPlayer() && IsPlayerInFOV())
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

        public bool IsCloseToPlayer()
    {
        return Vector3.Distance(PlayerController.PlayerTransform.position, transform.position) < alertDistance;
    }

    Vector3 VectorToPlayer()
    {
        return PlayerController.PlayerTransform.position - transform.position;
    }

    public bool IsPlayerInFOV()
    {
        return Vector3.Angle(transform.forward, VectorToPlayer()) < alertAngleTolerance;
    }

    public void LookAtPlayer()
    {
        Vector3 temp = VectorToPlayer();
        temp.y = 0;
        transform.forward = temp;
    }
}
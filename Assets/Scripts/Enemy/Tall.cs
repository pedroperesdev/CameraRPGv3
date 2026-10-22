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

    protected override void Awake()
    {
        base.Awake();
        Alerted = new AlertedState(this);
        Chase = new ChaseState(this);
        Confused = new ConfusedState(this);
        Patrol = new PatrolState(this);
    }
    void Start()
    {
        ChangeState(Patrol);
    }

    void ChangeState(TallState state)
    {
        if(CurrentState != null) CurrentState.ExitState();
        CurrentState = state;
        CurrentState.EnterState(); 
    }

    void Update()
    {
        CurrentState.UpdateState();
    }
}
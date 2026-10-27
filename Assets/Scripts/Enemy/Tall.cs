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
    private Collider c;


    // Parameters for this enemy subclass
    private float alertDistance = 20f;
    private float attackDistance = 3f;
    private float alertAngleTolerance = 65f;
    private float jumpForce = 100f;
    private float timeBeforeChase = 1f;
    private float patrolSpeed = 2.5f;
    private float chaseSpeed = 5f;
    private bool attackOnCooldown = false;
    private float attackCooldown = 1.5f;
    private float attackDamage = 50f;

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

    public void SetPatrolSpeed()
    {
        NMA.speed = patrolSpeed;
    }

    public void SetChaseSpeed()
    {
        NMA.speed = chaseSpeed;
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
        Vector3 toPlayer = PlayerController.PlayerHead.transform.position - head.transform.position;
        Debug.DrawLine(head.transform.position, PlayerController.PlayerHead.transform.position, Color.yellow);
        return toPlayer.normalized;
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
        return Physics.Raycast(head.position, VectorToPlayer(), out RaycastHit hit, alertDistance) && hit.collider.CompareTag("PlayerHead");
    }

    public void LookAtPlayer()
    {
        Vector3 temp = VectorToPlayer();
        temp.y = 0;
        transform.forward = temp;
    }

    public void WaitAndStartChase()
    {
        StartCoroutine(StartChase());
    }

    IEnumerator StartChase()
    {
        yield return new WaitForSeconds(timeBeforeChase);
        ChangeState(Chase);
    }

    // will probably add an attack state later when there's an animation and attack zone
    public void TryAttackPlayer()
    {
        if(Vector3.Distance(PlayerController.PlayerHead.transform.position, head.transform.position) < attackDistance)
        {
            if(!attackOnCooldown && Physics.Raycast(head.position, VectorToPlayer(), out RaycastHit hit, attackDistance) && (hit.collider.CompareTag("PlayerHead") || hit.collider.CompareTag("Player")))
            {
                Debug.Log("hit");
                DamagePlayer();
                StartCoroutine(StartAttackCooldown());
            }      
        }
    }

    IEnumerator StartAttackCooldown()
    {
        attackOnCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        attackOnCooldown = false;
    }

    void DamagePlayer()
    {
        PlayerController.PlayerTransform.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
    }

}
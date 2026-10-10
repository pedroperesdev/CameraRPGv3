using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    protected CharacterController cc;
    public string EnemyName;
    public float MaxHp;
    public float CurrHp {get; private set;}
    public float AlertRadius = 10f;
    private bool alerted = false;
    protected float speed = 2f;
    private float patrolInterval = 0.5f;
    private float g = -9.8f;
    private float vertVel = 0;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        CurrHp = MaxHp;
    }

    void Start()
    {
        StartCoroutine(Patrol());
    }

    //Enemies are all alerted the same way (by proximity), they just react differently
    IEnumerator Patrol()
    {
        while (!alerted)
        {
            yield return new WaitForSeconds(patrolInterval);
            if(Vector3.Distance(PlayerController.PlayerTransform.position, transform.position) < AlertRadius)
            {
                alerted = true;
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        CurrHp -= dmg;
        if(CurrHp <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        if(alerted)
        {
            CombatBehaviour();
        }
        Vector3 resultingMov = Vector3.zero;
        if(!cc.isGrounded)
        {
            resultingMov.y = vertVel * Time.deltaTime + 0.5f*g* Time.deltaTime * Time.deltaTime;
            vertVel = vertVel + g * Time.deltaTime; 
        }
        else
        {
            resultingMov.y = 0; vertVel = 0;
        }
        cc.Move(resultingMov);

    }
    public abstract void CombatBehaviour();
}
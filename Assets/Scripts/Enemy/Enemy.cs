using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    public NavMeshAgent NMA;
    public string EnemyName;
    public float MaxHp;
    public float CurrHp {get; private set;}

    protected virtual void Awake()
    {
        NMA = GetComponent<NavMeshAgent>();
        CurrHp = MaxHp;
    }

    void Start()
    {
        
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

    }
}
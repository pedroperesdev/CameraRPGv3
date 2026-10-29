using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    private HUDController hudc;
    public float maxHealth {get; private set;}
    public float currHealth {get; private set;}
    public static event Action<float> OnHealthChange;
    void Awake()
    {
        hudc = GetComponent<HUDController>();
        maxHealth = 100f;
        currHealth = maxHealth;
        OnHealthChange?.Invoke(currHealth/maxHealth);
    }

    void Update()
    {
        
    }

    public void IncreaseHealth(float value)
    {
        maxHealth += value;
        OnHealthChange?.Invoke(currHealth/maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currHealth -= amount;
        if(currHealth < 0) currHealth = 0;
        OnHealthChange?.Invoke(currHealth/maxHealth);
    }

    public void Heal(float amount)
    {
        currHealth += amount;
        if(currHealth > maxHealth) currHealth = maxHealth;
        OnHealthChange?.Invoke(currHealth/maxHealth);
    }
}
using UnityEngine;

/// <summary>
/// Interface for damageable entities.
/// There may be damageable entities in the future, apart from enemies, 
/// like crates or destructible items.
/// </summary>
public interface IDamageable
{
    public void TakeDamage(float dmg);
}
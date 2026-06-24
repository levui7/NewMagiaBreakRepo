using UnityEngine;

public class TrainingDummy : MonoBehaviour
{
    [Header("Dummy")]
    public int maxHealth = 999999;

    private int currentHealth;

    private StatusEffectController statusEffects;

    private void Awake()
    {
        currentHealth = maxHealth;
        statusEffects = GetComponent<StatusEffectController>();
    }

    public void TakeDamage(int damage, Element element)
    {
        currentHealth -= damage;

        DamagePopup2D.SpawnDamage(
            transform.position,
            damage,
            element);

        statusEffects?.ApplyElementStatus(element);

        // Не позволяем умереть
        if (currentHealth <= 0)
            currentHealth = maxHealth;
    }
}
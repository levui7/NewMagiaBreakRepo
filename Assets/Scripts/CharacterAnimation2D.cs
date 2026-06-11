using UnityEngine;

public class CharacterAnimation2D : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Parameter Names")]
    public string speedParameter = "Speed";
    public string attackTrigger = "Attack";
    public string takeDamageTrigger = "TakeDamage";
    public string dieTrigger = "Die";

    [Header("Settings")]
    public bool useLocalAnimatorIfEmpty = true;

    private bool isDead;

    private void Awake()
    {
        if (animator == null && useLocalAnimatorIfEmpty)
            animator = GetComponentInChildren<Animator>();
    }

    public void SetSpeed(float speed)
    {
        if (animator == null)
            return;

        if (isDead)
        {
            animator.SetFloat(speedParameter, 0f);
            return;
        }

        animator.SetFloat(speedParameter, Mathf.Abs(speed));
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;

        if (isDead)
            return;

        animator.SetTrigger(attackTrigger);
    }

    public void PlayTakeDamage()
    {
        if (animator == null)
            return;

        if (isDead)
            return;

        animator.SetTrigger(takeDamageTrigger);
    }

    public void PlayDeath()
    {
        if (animator == null)
            return;

        if (isDead)
            return;

        isDead = true;

        animator.SetFloat(speedParameter, 0f);
        animator.SetTrigger(dieTrigger);
    }

    public void ResetDeathState()
    {
        isDead = false;
    }
}
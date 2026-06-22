//using UnityEngine;

//public class CharacterAnimation2D : MonoBehaviour
//{
//    [Header("Animator")]
//    public Animator animator;

//    [Header("Parameter Names")]
//    public string speedParameter = "Speed";
//    public string attackTrigger = "Attack";
//    public string takeDamageTrigger = "TakeDamage";
//    public string dieTrigger = "Die";

//    [Header("Settings")]
//    public bool useLocalAnimatorIfEmpty = true;

//    private bool isDead;

//    private void Awake()
//    {
//        if (animator == null && useLocalAnimatorIfEmpty)
//            animator = GetComponentInChildren<Animator>();
//    }

//    public void SetSpeed(float speed)
//    {
//        if (animator == null)
//            return;

//        if (isDead)
//        {
//            animator.SetFloat(speedParameter, 0f);
//            return;
//        }

//        animator.SetFloat(speedParameter, Mathf.Abs(speed));
//    }

//    public void PlayAttack()
//    {
//        if (animator == null)
//            return;

//        if (isDead)
//            return;

//        animator.SetTrigger(attackTrigger);
//    }

//    public void PlayTakeDamage()
//    {
//        if (animator == null)
//            return;

//        if (isDead)
//            return;

//        animator.SetTrigger(takeDamageTrigger);
//    }

//    public void PlayDeath()
//    {
//        if (animator == null)
//            return;

//        if (isDead)
//            return;

//        isDead = true;

//        animator.SetFloat(speedParameter, 0f);
//        animator.SetTrigger(dieTrigger);
//    }

//    public void ResetDeathState()
//    {
//        isDead = false;
//    }
//}

using UnityEngine;

public class CharacterAnimation2D : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Parameter Names")]
    public string speedParameter = "Speed";
    public string attackTrigger = "Attack";

    // ❌ Можно удалить, если точно не нужны:
    // public string takeDamageTrigger = "TakeDamage";
    // public string dieTrigger = "Die";

    [Header("Settings")]
    public bool useLocalAnimatorIfEmpty = true;

    private bool isDead;

    private void Awake()
    {
        if (animator == null && useLocalAnimatorIfEmpty)
            animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// ✅ РАБОТАЕТ: управляет Idle/Run через параметр Speed
    /// </summary>
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

    /// <summary>
    /// ✅ РАБОТАЕТ: запускает анимацию атаки
    /// </summary>
    public void PlayAttack()
    {
        if (animator == null)
            return;

        if (isDead)
            return;

        animator.SetTrigger(attackTrigger);
    }

    /// <summary>
    /// ❌ ОТКЛЮЧЕНО: больше не запускает анимацию получения урона
    /// </summary>
    public void PlayTakeDamage()
    {
        // Ничего не делаем — анимация отключена
        return;
    }

    /// <summary>
    /// ❌ ОТКЛЮЧЕНО: не запускает анимацию смерти,
    /// но помечает объект как мёртвый (важно для логики игры)
    /// </summary>
    public void PlayDeath()
    {
        isDead = true;

        // Останавливаем движение в Animator (переход в Idle)
        if (animator != null)
            animator.SetFloat(speedParameter, 0f);

        return;
    }

    public void ResetDeathState()
    {
        isDead = false;
    }
}
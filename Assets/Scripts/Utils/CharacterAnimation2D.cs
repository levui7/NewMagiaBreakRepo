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



// НОВЫЙ 
//using UnityEngine;

//public class CharacterAnimation2D : MonoBehaviour
//{
//    [Header("Animator")]
//    public Animator animator;

//    [Header("Parameter Names")]
//    public string speedParameter = "Speed";
//    public string attackTrigger = "Attack";

//    // ❌ Можно удалить, если точно не нужны:
//    // public string takeDamageTrigger = "TakeDamage";
//    // public string dieTrigger = "Die";


//    [Header("Settings")]
//    public bool useLocalAnimatorIfEmpty = true;

//    private bool isDead;

//    private void Awake()
//    {
//        if (animator == null && useLocalAnimatorIfEmpty)
//            animator = GetComponentInChildren<Animator>();
//    }

//    /// <summary>
//    /// ✅ РАБОТАЕТ: управляет Idle/Run через параметр Speed
//    /// </summary>
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

//    /// <summary>
//    /// ✅ РАБОТАЕТ: запускает анимацию атаки
//    /// </summary>
//    public void PlayAttack()
//    {
//        if (animator == null)
//            return;

//        if (isDead)
//            return;

//        animator.SetTrigger(attackTrigger);
//    }

//    /// <summary>
//    /// ❌ ОТКЛЮЧЕНО: больше не запускает анимацию получения урона
//    /// </summary>
//    public void PlayTakeDamage()
//    {
//        // Ничего не делаем — анимация отключена
//        return;
//    }

//    /// <summary>
//    /// ❌ ОТКЛЮЧЕНО: не запускает анимацию смерти,
//    /// но помечает объект как мёртвый (важно для логики игры)
//    /// </summary>
//    public void PlayDeath()
//    {
//        isDead = true;

//        // Останавливаем движение в Animator (переход в Idle)
//        if (animator != null)
//            animator.SetFloat(speedParameter, 0f);

//        return;
//    }

//    public void ResetDeathState()
//    {
//        isDead = false;
//    }
//}


// НОВЫЙ 2
using UnityEngine;

public class CharacterAnimation2D : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Parameter Names")]
    public string speedParameter = "Speed";
    public string attackTrigger = "Attack";

    [Header("Flip Settings")]
    [Tooltip("Автоматически поворачивать персонажа при движении")]
    public bool flipOnMove = true;

    [Tooltip("Минимальное значение X для поворота (чтобы не дёргался на месте)")]
    public float flipThreshold = 0.1f;

    [Header("Settings")]
    public bool useLocalAnimatorIfEmpty = true;

    private bool isDead;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;  // Начальное направление (как в Player.cs)

    private void Awake()
    {
        if (animator == null && useLocalAnimatorIfEmpty)
            animator = GetComponentInChildren<Animator>();

        // Получаем SpriteRenderer для поворота
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// ✅ Управляет Idle/Run через параметр Speed
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
    /// ✅ НОВОЕ: Поворачивает персонажа в направлении движения
    /// Работает аналогично Flip() из Player.cs, но через SetDirection
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        if (!flipOnMove || isDead)
            return;

        // Игнорируем слишком маленькие значения (чтобы не дёргался)
        if (Mathf.Abs(direction.x) < flipThreshold)
            return;

        // Определяем, в какую сторону должен смотреть персонаж
        bool shouldFaceRight = direction.x > 0;

        // Поворачиваем только если направление изменилось
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            Flip();
        }
    }

    /// <summary>
    /// Переворачивает спрайт (аналог Flip() из Player.cs)
    /// Использует localScale.x *= -1 (как в твоём Player.cs)
    /// </summary>
    public void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    /// <summary>
    /// ✅ Запускает анимацию атаки
    /// </summary>
    public void PlayAttack()
    {
        if (animator == null || isDead)
            return;

        animator.SetTrigger(attackTrigger);
    }

    /// <summary>
    /// ❌ ОТКЛЮЧЕНО: анимация получения урона
    /// </summary>
    public void PlayTakeDamage()
    {
        return;
    }

    /// <summary>
    /// ❌ ОТКЛЮЧЕНО: анимация смерти
    /// </summary>
    public void PlayDeath()
    {
        isDead = true;

        if (animator != null)
            animator.SetFloat(speedParameter, 0f);
    }

    public void ResetDeathState()
    {
        isDead = false;
    }

    /// <summary>
    /// Принудительно установить направление (для внешних вызовов)
    /// </summary>
    public void SetFacingRight(bool right)
    {
        if (facingRight != right)
        {
            facingRight = right;
            Flip();
        }
    }
}
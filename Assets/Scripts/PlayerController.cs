using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    public int playerID = 1;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float dashSpeed = 14f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.8f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool godMode = false;

    [Header("Weapon")]
    public Transform weaponPivot;
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float fireRate = 0.2f;

    [Header("Aiming")]
    public bool autoAimEnabled = true;
    public float autoAimRadius = 12f;

    [Tooltip("Можно оставить Everything. Фильтрация всё равно идёт по Enemy/Boss.")]
    public LayerMask autoAimMask = ~0;

    public bool includeBossInAutoAim = true;

    [Tooltip("Если включено, автонаведение ищет цели на любых слоях. Это надёжнее, если враги стоят на Default.")]
    public bool searchAllLayersForAutoAim = true;

    [Tooltip("Если включено, игрок будет видеть радиус автонаведения в Scene View.")]
    public bool drawAutoAimGizmos = true;

    [Header("Manual Aim")]
    public bool player1UsesMouseAim = true;

    private Rigidbody2D rb;
    private WeaponManager weaponManager;

    private Vector2 moveInput;
    private Vector2 aimDirection = Vector2.right;

    private Transform currentAutoAimTarget;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float fireCooldownTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        weaponManager = GetComponent<WeaponManager>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (currentHealth <= 0f)
            currentHealth = maxHealth;

        // Если маска не задана, ставим Everything, чтобы автонаведение не зависело от Layer.
        if (autoAimMask.value == 0)
            autoAimMask = ~0;
    }

    private void Update()
    {
        ReadInput();
        HandleAim();
        HandleFire();
        HandleDashTimers();

        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHUD(this, weaponManager);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;

        moveInput = Vector2.zero;

        if (keyboard == null)
            return;

        if (playerID == 2)
        {
            if (keyboard.iKey.isPressed || keyboard.numpad8Key.isPressed)
                moveInput.y += 1f;

            if (keyboard.kKey.isPressed || keyboard.numpad5Key.isPressed || keyboard.numpad2Key.isPressed)
                moveInput.y -= 1f;

            if (keyboard.jKey.isPressed || keyboard.numpad4Key.isPressed)
                moveInput.x -= 1f;

            if (keyboard.lKey.isPressed || keyboard.numpad6Key.isPressed)
                moveInput.x += 1f;

            if ((keyboard.rightShiftKey.wasPressedThisFrame || keyboard.numpad0Key.wasPressedThisFrame) && dashCooldownTimer <= 0f)
                StartDash();
        }
        else
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                moveInput.y += 1f;

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                moveInput.y -= 1f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                moveInput.x -= 1f;

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                moveInput.x += 1f;

            if (keyboard.spaceKey.wasPressedThisFrame && dashCooldownTimer <= 0f)
                StartDash();
        }

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();
    }

    private void Move()
    {
        float speed = isDashing ? dashSpeed : moveSpeed;
        rb.linearVelocity = moveInput * speed;
    }

    private void HandleDashTimers()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (!isDashing)
            return;

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
            isDashing = false;
    }

    private void StartDash()
    {
        if (moveInput.sqrMagnitude <= 0.01f)
            return;

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
    }

    private void HandleAim()
    {
        Vector2? targetDirection = autoAimEnabled ? FindAutoAimDirection() : null;

        if (targetDirection.HasValue)
        {
            aimDirection = targetDirection.Value;
        }
        else if (playerID == 1 && player1UsesMouseAim && Mouse.current != null && Camera.main != null)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreen.x, mouseScreen.y, -Camera.main.transform.position.z)
            );

            Vector2 direction = (Vector2)(mouseWorld - transform.position);

            if (direction.sqrMagnitude > 0.001f)
                aimDirection = direction.normalized;
        }
        else if (moveInput.sqrMagnitude > 0.01f)
        {
            aimDirection = moveInput.normalized;
        }

        RotateTransformToDirection2D(weaponPivot, aimDirection);
    }

    private Vector2? FindAutoAimDirection()
    {
        currentAutoAimTarget = null;

        Collider2D[] hits;

        if (searchAllLayersForAutoAim)
        {
            // Самый надёжный вариант: ищем всё вокруг, потом фильтруем по Enemy/Boss.
            hits = Physics2D.OverlapCircleAll(transform.position, autoAimRadius);
        }
        else
        {
            // Вариант через маску. Используй только если точно настроены слои Enemy/Boss.
            hits = Physics2D.OverlapCircleAll(transform.position, autoAimRadius, autoAimMask);
        }

        Collider2D bestCollider = null;
        float bestDistanceSqr = autoAimRadius * autoAimRadius;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (!hit.gameObject.activeInHierarchy)
                continue;

            // Не целимся в себя и свои дочерние коллайдеры.
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            if (!IsValidAutoAimTarget(hit))
                continue;

            Vector2 targetPoint = GetColliderAimPoint(hit);
            Vector2 toTarget = targetPoint - (Vector2)transform.position;
            float distanceSqr = toTarget.sqrMagnitude;

            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                bestCollider = hit;
            }
        }

        if (bestCollider == null)
            return null;

        currentAutoAimTarget = bestCollider.transform;

        Vector2 bestPoint = GetColliderAimPoint(bestCollider);
        Vector2 directionToBestTarget = bestPoint - (Vector2)transform.position;

        if (directionToBestTarget.sqrMagnitude <= 0.001f)
            return null;

        return directionToBestTarget.normalized;
    }

    private Vector2 GetColliderAimPoint(Collider2D collider)
    {
        if (collider == null)
            return transform.position;

        return collider.bounds.center;
    }

    private bool IsValidAutoAimTarget(Collider2D hit)
    {
        if (hit == null)
            return false;

        if (hit.CompareTag("Enemy"))
            return true;

        if (includeBossInAutoAim && hit.CompareTag("Boss"))
            return true;

        if (hit.GetComponentInParent<Enemy>() != null)
            return true;

        if (includeBossInAutoAim && hit.GetComponentInParent<BossController>() != null)
            return true;

        return false;
    }

    private void HandleFire()
    {
        if (fireCooldownTimer > 0f)
            fireCooldownTimer -= Time.deltaTime;

        if (!IsFirePressed())
            return;

        if (fireCooldownTimer > 0f)
            return;

        Fire();
        fireCooldownTimer = fireRate;
    }

    private bool IsFirePressed()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (playerID == 2)
        {
            if (keyboard == null)
                return false;

            return keyboard.enterKey.isPressed ||
                   keyboard.numpadEnterKey.isPressed ||
                   keyboard.rightCtrlKey.isPressed;
        }

        bool mouseFire = mouse != null && mouse.leftButton.isPressed;
        bool keyboardFire = keyboard != null && keyboard.leftCtrlKey.isPressed;

        return mouseFire || keyboardFire;
    }

    private void Fire()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("PlayerController: Projectile Prefab не назначен.", this);
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning("PlayerController: Fire Point не назначен.", this);
            return;
        }

        if (weaponManager != null && !weaponManager.CanShoot())
        {
            Debug.LogWarning("PlayerController: CanShoot = false. " + weaponManager.GetAmmoText(), this);
            return;
        }

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        BulletScript bulletScript = bullet.GetComponent<BulletScript>();

        if (bulletScript != null)
        {
            float damage = weaponManager != null ? weaponManager.GetCurrentDamage() : 10f;
            WeaponManager.Element element = weaponManager != null ? weaponManager.CurrentElement : WeaponManager.Element.Physical;

            bulletScript.SetDirection(aimDirection);
            bulletScript.SetDamage(damage);
            bulletScript.SetElement(element);
            bulletScript.SetOwner(gameObject);
        }
        else
        {
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null)
                bulletRb.linearVelocity = aimDirection * 12f;
        }

        weaponManager?.ConsumeAmmo();
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, WeaponManager.Element.Physical);
    }

    public void TakeDamage(float amount, WeaponManager.Element element)
    {
        if (godMode)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        DamagePopup2D.SpawnDamage(transform.position, Mathf.CeilToInt(amount), element);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHUD(this, weaponManager);

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        DamagePopup2D.SpawnHeal(transform.position, Mathf.CeilToInt(amount));

        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHUD(this, weaponManager);
    }

    public int GetCurrentHealth()
    {
        return Mathf.CeilToInt(currentHealth);
    }

    public int GetMaxHealth()
    {
        return Mathf.CeilToInt(maxHealth);
    }

    public float GetCurrentHealthFloat()
    {
        return currentHealth;
    }

    public float GetMaxHealthFloat()
    {
        return maxHealth;
    }

    public float GetHealth01()
    {
        if (maxHealth <= 0f)
            return 0f;

        return currentHealth / maxHealth;
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHUD(this, weaponManager);
    }

    public void SetHealth(int value)
    {
        SetHealth((float)value);
    }

    public void SetHealth(float value, bool clampToMax)
    {
        if (clampToMax)
            currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        else
            currentHealth = Mathf.Max(0f, value);

        if (UIManager.Instance != null)
            UIManager.Instance.UpdatePlayerHUD(this, weaponManager);
    }

    private void Die()
    {
        rb.linearVelocity = Vector2.zero;

        GameSessionManager session = GameSessionManager.Instance;

        if (session == null)
            session = FindObjectOfType<GameSessionManager>();

        if (session != null)
        {
            session.OnPlayerDied(this);
        }
        else
        {
            Debug.LogError("PlayerController: GameSessionManager не найден. Экран смерти не может быть загружен.");
        }

        gameObject.SetActive(false);
    }

    public static void RotateTransformToDirection2D(Transform targetTransform, Vector2 direction)
    {
        if (targetTransform == null)
            return;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        targetTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public static void RotateTransformToDirection2D(Transform targetTransform, Vector3 direction)
    {
        RotateTransformToDirection2D(targetTransform, new Vector2(direction.x, direction.y));
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawAutoAimGizmos)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, autoAimRadius);

        if (Application.isPlaying && currentAutoAimTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentAutoAimTarget.position);
        }
    }
}
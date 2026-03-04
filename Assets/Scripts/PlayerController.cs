using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private Vector2 moveDirection;
    [SerializeField] private float damage;
    [SerializeField] LayerMask Attackable;
    [SerializeField] public PlayerStateList pState; // Ensure this is serialized
    [SerializeField] float HitFlashSpeed;
    public int health;
    public int maxHealth;
    public Image healthBar;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    public static PlayerController Instance;

    private float lastDamageTime;
    private Coroutine healthRegenCoroutine;

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCooldown;
    private bool isDashing = false;
    private bool canDash = true;
    private float dashTime;
    [SerializeField] public float distanceBetweenAfterImages;
    private float lastAfterImageX;
    private bool isMachinegunActive = false;
    private float machinegunDuration = 10f; // Duration for which the machine gun is active
    private float machinegunTimer = 0f;
    public GameObject normalGun;
    public GameObject machineGun;
    private PlayerShooting playerShooting;
    private PlayerMachineGunShooting playerMachineGunShooting;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        health = maxHealth;
        lastDamageTime = Time.time;
        pState.invincible = false;
        playerShooting = normalGun.GetComponent<PlayerShooting>();
        playerMachineGunShooting = machineGun.GetComponent<PlayerMachineGunShooting>();
    }

    private void Update()
    {
        ProcessMovementInputs();
        ProcessShootingInputs();
        FlashWhileInvincible();
        Move();
        healthBar.fillAmount = Mathf.Clamp((float)health / maxHealth, 0, 1);

        // Start health regeneration if not already running
        if (Time.time - lastDamageTime > 2f && healthRegenCoroutine == null)
        {
            healthRegenCoroutine = StartCoroutine(HealthRegen());
        }

        if (isDashing)
        {
            dashTime -= Time.deltaTime;
            if (dashTime <= 0)
            {
                isDashing = false;
                StartCoroutine(DashCooldown());
            }
        }
        if (isMachinegunActive)
        {
            machinegunTimer -= Time.deltaTime;
            if (machinegunTimer <= 0)
            {
                DeactivateMachinegun();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = moveDirection * dashSpeed;
            if (Mathf.Abs(transform.position.x - lastAfterImageX) > distanceBetweenAfterImages)
            {
                DashAfterImagePool.Instance.GetFromPool();
                lastAfterImageX = transform.position.x;
            }
        }
        else
        {
            Move();
        }
    }

    private void ProcessMovementInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && moveDirection != Vector2.zero)
        {
            Dash();
        }
    }

    private void ProcessShootingInputs()
    {
        float shootX = 0;
        float shootY = 0;

        if (Input.GetKey(KeyCode.J))
        {
            shootX = -1; // Left
        }
        else if (Input.GetKey(KeyCode.L))
        {
            shootX = 1; // Right
        }

        if (Input.GetKey(KeyCode.I))
        {
            shootY = 1; // Up
        }
        else if (Input.GetKey(KeyCode.K))
        {
            shootY = -1; // Down
        }

        Vector2 shootDirection = new Vector2(shootX, shootY).normalized;

        if (shootDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            if (isMachinegunActive)
            {
                playerMachineGunShooting.SetRotationAndFire(angle);
            }
            else
            {
                playerShooting.SetRotationAndFire(angle);
            }
        }
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);

        bool IsWalking = moveDirection != Vector2.zero;
        anim.SetBool("IsWalking", IsWalking);

        if (moveDirection.x > 0)
        {
            sr.flipX = false; // Face right
        }
        else if (moveDirection.x < 0)
        {
            sr.flipX = true; // Face left
        }
    }
    private void Dash()
    {
        isDashing = true;
        canDash = false;
        dashTime = dashDuration;
        pState.invincible = true; // Set invincible to true when dashing
        anim.SetBool("IsDashing", true); // Set IsDashing to true when dashing
        SetLayerCollision(false);
        DashAfterImagePool.Instance.GetFromPool();
        lastAfterImageX = transform.position.x;
        StartCoroutine(ResetInvincibility());
    }

    private IEnumerator ResetInvincibility()
    {
        yield return new WaitForSeconds(dashDuration);
        pState.invincible = false; // Set invincible back to false after dash duration
        anim.SetBool("IsDashing", false); // Set IsDashing back to false after dash duration
        SetLayerCollision(true);
    }

    private void SetLayerCollision(bool enable)
    {
        int playerLayer = gameObject.layer;
        int attackableLayer = LayerMask.NameToLayer("Attackable");
        Physics2D.IgnoreLayerCollision(playerLayer, attackableLayer, !enable);
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }

                // Update last damage time and stop health regeneration if health is reduced
                if (health < value)
                {
                    lastDamageTime = Time.time;
                    if (healthRegenCoroutine != null)
                    {
                        StopCoroutine(healthRegenCoroutine);
                        healthRegenCoroutine = null;
                    }
                }
            }
        }
    }

    public void ResetHealthRegenTimer()
    {
        lastDamageTime = Time.time;
        if (healthRegenCoroutine != null)
        {
            StopCoroutine(healthRegenCoroutine);
            healthRegenCoroutine = null;
        }
    }

    IEnumerator StopTakingDamage()
    {
        if (pState != null)
        {
            pState.invincible = true;
            yield return new WaitForSeconds(0.6f);
            pState.invincible = false;
        }
    }

    void FlashWhileInvincible()
    {
        if (sr == null || pState == null)
        {
            return;
        }

        sr.material.color = pState.invincible ? Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * HitFlashSpeed, 1.0f)) : Color.white;
    }

    public void TakeDamage(float _damage)
    {
        if (pState != null && !pState.invincible)
        {
            Health -= Mathf.RoundToInt(_damage);
            StartCoroutine(StopTakingDamage());

            // Reset the last damage time and stop health regeneration coroutine
            ResetHealthRegenTimer();
        }
    }

    IEnumerator HealthRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            if (Time.time - lastDamageTime > 2f)
            {
                Health += 2;
            }
            else
            {
                healthRegenCoroutine = null;
                yield break;
            }
        }
    }
    public void ActivateMachinegun()
    {
        isMachinegunActive = true;
        machinegunTimer = machinegunDuration;
        normalGun.SetActive(false); // Hide the normal gun
        machineGun.SetActive(true); // Unhide the machine gun
        Debug.Log("Machine gun activated!");
    }

    private void DeactivateMachinegun()
    {
        isMachinegunActive = false;
        normalGun.SetActive(true); // Unhide the normal gun
        machineGun.SetActive(false); // Hide the machine gun
        Debug.Log("Machine gun deactivated!");
    }
}
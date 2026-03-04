using UnityEngine;

public class GalaxySwordAttack : MonoBehaviour
{
    [Header("Damage")]
    public float damage;
    [SerializeField] private float knockbackForce = 100f;

    [Header("Wind-up (Above Player)")]
    [SerializeField] private float windUpTime = 2f;
    [SerializeField] private Vector2 hoverOffset = new Vector2(0f, 1f);
    [SerializeField] private float spinDegreesPerSecond = 360f;
    [SerializeField] private float summonRiseTime = 0.15f;

    [Header("Launch")]
    [SerializeField] private float launchSpeed = 12f;
    [SerializeField] private float lifetimeAfterLaunch = 4f;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float spriteAngleOffsetDegrees = 45f;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionRadius = 1.25f;
    [SerializeField] private float explosionEffectLifetime = 0.34f;
    [SerializeField] private float explosionDamage = 0f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Transform player;
    private bool hasLaunched;
    private bool hasExploded;
    private float stateTimer;
    private Vector2 fallbackLaunchDirection;
    private Vector2 windUpStartPosition;
    private Vector2 windUpTargetPosition;

    public void Init(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (player == null)
        {
            if (PlayerController.Instance != null)
            {
                player = PlayerController.Instance.transform;
            }
            else
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }
        }

        fallbackLaunchDirection = GetFallbackDirection();
        BeginWindUp();
    }

    private void Update()
    {
        if (!hasLaunched)
        {
            WindUpUpdate();
        }
        else
        {
            LaunchUpdate();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded)
        {
            return;
        }

        if (other.gameObject.CompareTag(enemyTag))
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.EnemyHit(damage, (transform.position - other.transform.position).normalized, knockbackForce);
            }

            Vector3 explosionPosition = other.ClosestPoint(transform.position);
            Explode(explosionPosition, other);
        }
    }

    private void BeginWindUp()
    {
        hasLaunched = false;
        stateTimer = 0f;

        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        if (col != null)
        {
            col.enabled = false;
        }

        if (player != null)
        {
            windUpStartPosition = player.position;
            windUpTargetPosition = (Vector2)player.position + hoverOffset;
        }
        else
        {
            windUpStartPosition = transform.position;
            windUpTargetPosition = transform.position;
        }

        transform.position = windUpStartPosition;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.angularVelocity = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void WindUpUpdate()
    {
        stateTimer += Time.deltaTime;

        if (summonRiseTime > 0f && stateTimer < summonRiseTime)
        {
            float t = Mathf.Clamp01(stateTimer / summonRiseTime);
            transform.position = Vector2.Lerp(windUpStartPosition, windUpTargetPosition, t);
        }
        else
        {
            transform.position = windUpTargetPosition;
        }

        transform.Rotate(0f, 0f, spinDegreesPerSecond * Time.deltaTime);

        if (stateTimer >= windUpTime)
        {
            LaunchTowardsNearestEnemy();
        }
    }

    private void LaunchUpdate()
    {
        stateTimer += Time.deltaTime;

        if (rb == null)
        {
            transform.position += (Vector3)(fallbackLaunchDirection * launchSpeed * Time.deltaTime);
            RotateTowards(fallbackLaunchDirection);
        }
        else
        {
            RotateTowardsMovementDirection();
        }

        if (stateTimer >= lifetimeAfterLaunch)
        {
            Destroy(gameObject);
        }
    }

    private void LaunchTowardsNearestEnemy()
    {
        hasLaunched = true;
        stateTimer = 0f;

        if (col != null)
        {
            col.enabled = true;
        }

        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        Transform target = FindNearestEnemyTransform();
        Vector2 direction;

        if (target != null)
        {
            direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            if (direction == Vector2.zero)
            {
                direction = fallbackLaunchDirection;
            }
        }
        else
        {
            direction = fallbackLaunchDirection;
        }

        fallbackLaunchDirection = direction;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * launchSpeed;
        }
        else
        {
            RotateTowards(direction);
        }
    }

    private Transform FindNearestEnemyTransform()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies == null || enemies.Length == 0)
        {
            return null;
        }

        Transform nearest = null;
        float minDistance = Mathf.Infinity;
        Vector2 currentPosition = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            float distance = Vector2.Distance(currentPosition, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    private Vector2 GetFallbackDirection()
    {
        if (PlayerShooting.Instance != null)
        {
            float angleRad = PlayerShooting.Instance.lastShootingAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            if (dir != Vector2.zero)
            {
                return dir.normalized;
            }
        }

        return Vector2.right;
    }

    private void RotateTowardsMovementDirection()
    {
        if (rb == null)
        {
            return;
        }

        Vector2 direction = rb.linearVelocity;
        if (direction != Vector2.zero)
        {
            RotateTowards(direction);
        }
    }

    private void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffsetDegrees);
    }

    private void Explode(Vector3 explosionPosition, Collider2D directHit)
    {
        hasExploded = true;

        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, explosionPosition, Quaternion.identity);
            Destroy(effect, explosionEffectLifetime);
        }

        float aoeDamage = explosionDamage > 0f ? explosionDamage : damage;
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == directHit) continue;
            if (!hit.CompareTag(enemyTag)) continue;

            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.EnemyHit(aoeDamage, (explosionPosition - hit.transform.position).normalized, knockbackForce);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
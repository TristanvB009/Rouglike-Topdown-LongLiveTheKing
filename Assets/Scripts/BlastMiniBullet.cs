using UnityEngine;
using System.Collections;

public class BlastMiniBullet : MonoBehaviour
{
    public float speed;
    public float homingStrength;
    public float damage;
    public GameObject explosionEffectPrefab; // Explosion effect prefab

    [Header("Homing Timing")]
    [SerializeField] private float homingDelay = 0f;
    [SerializeField] private bool synchronizeArrival = true;
    [SerializeField] private float desiredHomingTime = 0.6f;
    [SerializeField] private float minSpeedMultiplier = 0.75f;
    [SerializeField] private float maxSpeedMultiplier = 2.25f;

    [Header("Arc Before Homing")]
    [SerializeField] private float orbitDuration = 0.35f;
    [SerializeField] private float orbitSpeedMultiplier = 0.6f;
    [SerializeField] private float orbitTurnStrengthMultiplier = 2f;
    [SerializeField] private float orbitRadialCorrection = 2.5f;

    private Rigidbody2D rb;
    private Collider2D hitCollider;
    private Transform target;
    private float timer = 0f;
    private bool hasHitEnemy = false;
    private bool canHome = false; // Flag to start homing
    private bool canOrbit = false;
    private bool orbitClockwise = true;

    private float currentSpeed;
    private float orbitTimer;
    private float orbitRadius;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hitCollider = GetComponent<Collider2D>();
        currentSpeed = speed;
        rb.linearVelocity = Vector2.zero;
        StartCoroutine(EnableCollisionAfterDelay(0.3f));
        StartCoroutine(StartHomingAfterDelay(homingDelay));
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 4f)
        {
            Destroy(gameObject);
        }

        if (canOrbit && target != null)
        {
            orbitTimer += Time.deltaTime;

            Vector2 toTarget = (Vector2)target.position - rb.position;
            float dist = toTarget.magnitude;
            if (dist > 0.0001f)
            {
                Vector2 toTargetNorm = toTarget / dist;
                Vector2 tangent = orbitClockwise
                    ? new Vector2(toTargetNorm.y, -toTargetNorm.x)
                    : new Vector2(-toTargetNorm.y, toTargetNorm.x);

                float radialError = orbitRadius - dist;
                Vector2 desiredDir = (tangent + toTargetNorm * (radialError * orbitRadialCorrection)).normalized;

                float rotateAmount = Vector3.Cross(desiredDir, transform.up).z;
                rb.angularVelocity = -rotateAmount * homingStrength * orbitTurnStrengthMultiplier;
                rb.linearVelocity = transform.up * currentSpeed;
            }

            if (orbitTimer >= orbitDuration)
            {
                canOrbit = false;
                canHome = true;

                if (synchronizeArrival && desiredHomingTime > 0f)
                {
                    float distance = Vector2.Distance(transform.position, target.position);
                    float desiredSpeed = distance / desiredHomingTime;
                    float minSpeed = speed * minSpeedMultiplier;
                    float maxSpeed = speed * maxSpeedMultiplier;
                    currentSpeed = Mathf.Clamp(desiredSpeed, minSpeed, maxSpeed);
                }
            }
        }
        else if (canHome && target != null)
        {
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();
            float rotateAmount = Vector3.Cross(direction, transform.up).z;
            float strengthMultiplier = speed > 0f ? (currentSpeed / speed) : 1f;
            rb.angularVelocity = -rotateAmount * homingStrength * strengthMultiplier;
            rb.linearVelocity = transform.up * currentSpeed;
        }
    }

    public void Initialize(Transform forcedTarget, float forcedHomingDelay, float forcedDesiredHomingTime, bool forcedSynchronizeArrival, float forcedOrbitDuration, bool forcedOrbitClockwise)
    {
        if (forcedTarget != null)
        {
            target = forcedTarget;
        }

        homingDelay = forcedHomingDelay;
        desiredHomingTime = forcedDesiredHomingTime;
        synchronizeArrival = forcedSynchronizeArrival;
        orbitDuration = forcedOrbitDuration;
        orbitClockwise = forcedOrbitClockwise;
    }

    public void Initialize(Transform forcedTarget, float forcedHomingDelay, float forcedDesiredHomingTime, bool forcedSynchronizeArrival, float forcedOrbitDuration)
    {
        Initialize(forcedTarget, forcedHomingDelay, forcedDesiredHomingTime, forcedSynchronizeArrival, forcedOrbitDuration, true);
    }

    public void SetOrbitClockwise(bool clockwise)
    {
        orbitClockwise = clockwise;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitEnemy) return;

        if (other.gameObject.CompareTag("Ground"))
        {
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            Explode(collisionPoint);
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            hasHitEnemy = true;
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            Explode(collisionPoint);
        }
    }

    private void Explode(Vector3 explosionPosition)
    {
        // Instantiate explosion effect
        GameObject explosionEffect = Instantiate(explosionEffectPrefab, explosionPosition, Quaternion.identity);
        explosionEffect.transform.localScale *= 0.5f; // Halve the scale of the effect

        // Deal damage to the enemy
        Collider2D[] hits = Physics2D.OverlapCircleAll(explosionPosition, 0.1f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.EnemyHit(damage, (explosionPosition - hit.transform.position).normalized, 100);
                }
            }
        }

        // Destroy the explosion effect after 0.34 seconds
        Destroy(explosionEffect, 0.34f);

        // Destroy the blast mini bullet
        Destroy(gameObject);
    }

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        return closestEnemy;
    }

    private IEnumerator EnableCollisionAfterDelay(float delay)
    {
        hitCollider.enabled = false;
        yield return new WaitForSeconds(delay);
        hitCollider.enabled = true;
    }

    private IEnumerator StartHomingAfterDelay(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (target == null)
        {
            target = FindClosestEnemy();
        }

        if (target != null && orbitDuration > 0f)
        {
            orbitRadius = Vector2.Distance(transform.position, target.position);
            orbitTimer = 0f;
            currentSpeed = Mathf.Max(0.01f, speed * orbitSpeedMultiplier);
            canOrbit = true;
            canHome = false;
            yield break;
        }

        // No orbit phase, go straight to homing
        if (synchronizeArrival && target != null && desiredHomingTime > 0f)
        {
            float distance = Vector2.Distance(transform.position, target.position);
            float desiredSpeed = distance / desiredHomingTime;
            float minSpeed = speed * minSpeedMultiplier;
            float maxSpeed = speed * maxSpeedMultiplier;
            currentSpeed = Mathf.Clamp(desiredSpeed, minSpeed, maxSpeed);
        }

        canHome = true;
    }

    private void OnDrawGizmos()
    {
        // Draw a wire circle for the explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}
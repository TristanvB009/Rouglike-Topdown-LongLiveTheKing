using UnityEngine;

public class BlastBullet : MonoBehaviour
{
    public float force;
    public float damage;
    public float explosionRadius;
    public GameObject explosionEffectPrefab;
    public GameObject miniBulletPrefab; // Mini bullet prefab
    public float miniBulletOffset = 1f; // Offset distance for mini bullets
    [Header("Mini Bullet Homing")]
    public bool synchronizeMiniBulletArrival = true;
    public float miniBulletHomingDelay = 0f;
    public float miniBulletDesiredHomingTime = 0.6f;
    public float miniBulletOrbitDuration = 0.35f;
    public bool spawnMiniBulletsAroundTarget = true;
    public bool miniBulletOrbitClockwise = true;
    private bool hasHitEnemy = false;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * force * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitEnemy) return;

        if (other.gameObject.CompareTag("Ground") || other.gameObject.CompareTag("Enemy"))
        {
            hasHitEnemy = true;
            Transform forcedTarget = other.gameObject.CompareTag("Enemy") ? other.transform : null;
            Explode(forcedTarget);
        }
    }

    private void Explode(Transform forcedTarget)
    {
        // Instantiate explosion effect
        GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        // Deal damage to enemies in the explosion radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.EnemyHit(damage, (transform.position - hit.transform.position).normalized, 100);
                }
            }
        }

        if (forcedTarget == null)
        {
            forcedTarget = FindClosestEnemyTransform(transform.position);
        }

        // Instantiate 5 mini bullets in a star pattern
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f; // 360 degrees / 5 bullets = 72 degrees per bullet

            Vector2 offsetDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 spawnCenter = (forcedTarget != null && spawnMiniBulletsAroundTarget) ? forcedTarget.position : transform.position;
            Vector3 offset = (Vector3)(offsetDir * miniBulletOffset);

            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            if (forcedTarget != null && spawnMiniBulletsAroundTarget)
            {
                // Face tangent so bullets immediately start orbiting around the target together.
                Vector2 tangent = miniBulletOrbitClockwise
                    ? new Vector2(offsetDir.y, -offsetDir.x)
                    : new Vector2(-offsetDir.y, offsetDir.x);
                float z = Vector2.SignedAngle(Vector2.up, tangent);
                rotation = Quaternion.Euler(0f, 0f, z);
            }

            GameObject mini = Instantiate(miniBulletPrefab, spawnCenter + offset, rotation);
            BlastMiniBullet miniBullet = mini.GetComponent<BlastMiniBullet>();
            if (miniBullet != null)
            {
                float delay = (forcedTarget != null && spawnMiniBulletsAroundTarget) ? 0f : miniBulletHomingDelay;
                miniBullet.Initialize(forcedTarget, delay, miniBulletDesiredHomingTime, synchronizeMiniBulletArrival, miniBulletOrbitDuration);
                miniBullet.SetOrbitClockwise(miniBulletOrbitClockwise);
            }
        }

        Destroy(explosionEffect, 0.34f);

        // Destroy the blast bullet
        Destroy(gameObject);
    }

    private Transform FindClosestEnemyTransform(Vector2 fromPosition)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            float distance = Vector2.Distance(fromPosition, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    private void OnDrawGizmos()
    {
        // Draw a wire circle for the explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
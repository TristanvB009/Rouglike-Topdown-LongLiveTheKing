using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RicoShotBulletScript : MonoBehaviour
{
    public float force;
    public float damage;
    private float timer;
    private bool hasHitEnemy;
    public GameObject bulletSprite; // Reference to the child GameObject containing the sprite
    public int maxBounces;
    private int bounceCount = 0;
    private Vector3 direction;

    // Start is called before the first frame update
    void Start()
    {
        direction = transform.right;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 3)
        {
            Destroy(gameObject);
        }

        // Move the bullet
        transform.position += direction * force * Time.deltaTime;

        // Check for collisions
        CheckCollisions();
    }

void CheckCollisions()
{
    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, force * Time.deltaTime);
    if (hit.collider != null)
    {
        // Ignore collisions with Player and EnemyProjectile tags
        if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("EnemyProjectile") || hit.collider.CompareTag("PlayerBullet"))
        {
            Physics2D.IgnoreCollision(hit.collider, GetComponent<Collider2D>());
            return;
        }

        bounceCount++;
        if (bounceCount >= maxBounces)
        {
            Destroy(gameObject);
            return;
        }

        if (hit.collider.CompareTag("Enemy"))
        {
            if (!hasHitEnemy)
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.EnemyHit(damage, (transform.position - hit.transform.position).normalized, 100);
                }
            }
        }

        // Find the next nearest enemy
        Enemy nextEnemy = FindNearestEnemy();
        if (nextEnemy != null)
        {
            // Change direction towards the next nearest enemy
            direction = (nextEnemy.transform.position - transform.position).normalized;
            RotateTowards(direction);
        }
        else
        {
            // Reflect the bullet's direction based on the collision normal
            Vector2 reflectDir = Vector2.Reflect(direction, hit.normal);

            // Add a random variation to the reflection angle within a 5-degree range
            float angleVariation = Random.Range(-5f, 5f);
            reflectDir = Quaternion.Euler(0, 0, angleVariation) * reflectDir;

            direction = reflectDir.normalized;
        }
    }
}
    void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    Enemy FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;
        Vector2 currentPosition = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(currentPosition, enemy.transform.position);
            if (distance < minDistance)
            {
                nearest = enemy;
                minDistance = distance;
            }
        }

        return nearest != null ? nearest.GetComponent<Enemy>() : null;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpriteFlash;

public class Flamer : Enemy
{
    public Transform topBulletTransform;
    public Transform middleBulletTransform;
    public Transform bottomBulletTransform;
    private bool isFlipping = false;
    public GameObject weakPoint; // Weak point GameObject
    public GameObject explosionEffectPrefab; // Explosion effect prefab
    public float ExplosionDamage; // Damage dealt by the explosion
    public float ExplosionRadius; // Radius of the explosion
    public GameObject flamethrower; // Flamethrower GameObject
    private GameObject player;
    private Animator animator;
    private SimpleFlash simpleFlash;
    private float timer;
    private float initialFlamethrowerRotation;
    private bool isFlipped = false; // Track the flipped state

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        simpleFlash = GetComponent<SimpleFlash>();
        initialFlamethrowerRotation = flamethrower.transform.eulerAngles.z;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        float distance = Vector2.Distance(transform.position, player.transform.position);
        timer += Time.deltaTime;

        if(timer > 0.05f)
        {
            timer = 0;
            shoot();
        }
        ChasePlayer();
    }

    private void shoot()
    {
        Invoke("FireBullets", 0.0f);
    }

    private void FireBullets()
    {
        // Add a random spread to each bullet's rotation
        float middleBulletSpread = Random.Range(-4f, 4f);
        float topBulletSpread = Random.Range(-4f, 4f);
        float bottomBulletSpread = Random.Range(-4f, 4f);

        Quaternion middleBulletRotation = middleBulletTransform.rotation * Quaternion.Euler(0, 0, middleBulletSpread);
        GameObject middleBullet = FireBallPool.Instance.GetFromPool();
        middleBullet.transform.position = middleBulletTransform.position;
        middleBullet.transform.rotation = middleBulletRotation;

        Quaternion topBulletRotation = middleBulletTransform.rotation * Quaternion.Euler(0, 0, -6 + topBulletSpread);
        GameObject topBullet = FireBallPool.Instance.GetFromPool();
        topBullet.transform.position = topBulletTransform.position;
        topBullet.transform.rotation = topBulletRotation;

        Quaternion bottomBulletRotation = middleBulletTransform.rotation * Quaternion.Euler(0, 0, 6 + bottomBulletSpread);
        GameObject bottomBullet = FireBallPool.Instance.GetFromPool();
        bottomBullet.transform.position = bottomBulletTransform.position;
        bottomBullet.transform.rotation = bottomBulletRotation;
    }

    public void WeakPointHit()
    {
        // Instantiate explosion effect
        GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        // Deal damage to the player and enemies in a radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, ExplosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController.Instance.TakeDamage(ExplosionDamage);
            }
            else if (hit.gameObject.CompareTag("Enemy"))
            {
                Enemy enemy = hit.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.EnemyHit(ExplosionDamage, (transform.position - hit.transform.position).normalized, 100);
                }
            }
        }

        Destroy(gameObject);
        Destroy(explosionEffect, 1f);
    }

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
        simpleFlash.Flash();
    }

    private void ChasePlayer()
    {
        Vector2 targetPosition = PlayerController.Instance.transform.position;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        RotateTowards(direction);
        Flip(direction);
    }

    private void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float clampedAngle = Mathf.Clamp(angle, initialFlamethrowerRotation - 90, initialFlamethrowerRotation + 90);
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, clampedAngle));
        flamethrower.transform.rotation = Quaternion.RotateTowards(flamethrower.transform.rotation, targetRotation, 250 * Time.deltaTime); // Adjust the rotation speed as needed
    }

    private void Flip(Vector2 direction)
    {
        if (!isFlipping && (direction.x < 0 && transform.localScale.x > 0 || direction.x > 0 && transform.localScale.x < 0))
        {
            StartCoroutine(FlipWithDelay(direction));
        }
    }

    private IEnumerator FlipWithDelay(Vector2 direction)
    {
        isFlipping = true;

        float elapsedTime = 0f;
        Vector2 initialPosition = transform.position;

        while (elapsedTime < 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, initialPosition + direction, speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        Vector3 flamethrowerRotation = flamethrower.transform.eulerAngles;
        flamethrower.transform.eulerAngles = flamethrowerRotation;

        // Update the initial rotation to reflect the flip
        initialFlamethrowerRotation = (initialFlamethrowerRotation + 180) % 360;
        isFlipped = !isFlipped; // Toggle the flipped state

        isFlipping = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
    }
}
using UnityEngine;
using SpriteFlash;
using System.Collections;

public class ReaperEnemy : Enemy
{
    public Transform attackRangeTransform; // Transform representing the attack range
    public float attackDamage = 10f; // Damage dealt by the attack
    public float attackCooldown = 1f; // Cooldown time between attacks
    public GameObject scythe; // Reference to the Scythe GameObject

    private SimpleFlash simpleFlash;
    private float attackTimer = 0f; // Timer to track cooldown
    private Animator scytheAnimator;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start(); // Call the base class Awake method to initialize 'rb'
        simpleFlash = GetComponent<SimpleFlash>();
        if (scythe != null)
        {
            scytheAnimator = scythe.GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (PlayerController.Instance != null)
        {
            ChasePlayer();
            HandleAttack();
        }
    }

    private void ChasePlayer()
    {
        Vector2 targetPosition = PlayerController.Instance.transform.position;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        RotateTowards(direction);
    }

    private void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void HandleAttack()
    {
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
        else if (IsPlayerInAttackRange())
        {
            AttackPlayer();
            attackTimer = attackCooldown; // Reset the cooldown timer
        }
    }

    private bool IsPlayerInAttackRange()
    {
        if (attackRangeTransform == null)
        {
            Debug.LogWarning("Attack range transform is not assigned.");
            return false;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackRangeTransform.position, attackRangeTransform.localScale, 0);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

private void AttackPlayer()
{
    if (scytheAnimator != null)
    {
        scytheAnimator.SetBool("IsAttacking", true);
        StartCoroutine(DelayedAttack());
    }
}

private IEnumerator DelayedAttack()
{
    yield return new WaitForSeconds(0.18f);
    PlayerController.Instance.TakeDamage(attackDamage);
    StartCoroutine(ResetIsAttacking());
}

private IEnumerator ResetIsAttacking()
{
    yield return new WaitForSeconds(0.5f);
    if (scytheAnimator != null)
    {
        scytheAnimator.SetBool("IsAttacking", false);
    }
}

    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
        simpleFlash.Flash();
    }

    private void OnDrawGizmos()
    {
        if (attackRangeTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackRangeTransform.position, attackRangeTransform.localScale);
        }
    }
}
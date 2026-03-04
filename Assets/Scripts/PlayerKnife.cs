using UnityEngine;

public class PlayerKnife : MonoBehaviour
{
    public float force;
    public float damage;
    public float spinSpeed; // Variable speed for spinning
    public float returnSpeed; // Speed at which the knife returns to the player
    private float timer = 0f;
    private bool hasHitEnemy = false;
    private bool isSpinning = false;
    private bool isReturning = false;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 0.2f && !isSpinning && !isReturning)
        {
            isSpinning = true;
        }

        if (isReturning)
        {
            ReturnToPlayer();
        }
        else if (!isSpinning)
        {
            transform.Translate(Vector3.right * force * Time.deltaTime);
        }
        else
        {
            transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitEnemy) return;

        if (other.gameObject.CompareTag("Ground"))
        {
            isSpinning = true;
        }
        else if (other.gameObject.CompareTag("Enemy"))
        {
            hasHitEnemy = true;
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.EnemyHit(damage, (transform.position - other.transform.position).normalized, 100);
            }
            Destroy(gameObject);
        }
    }

    public void StartReturning()
    {
        isReturning = true;
        isSpinning = false;
    }

    private void ReturnToPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.Translate(direction * returnSpeed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, playerTransform.position) < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}
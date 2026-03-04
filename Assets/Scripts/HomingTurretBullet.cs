using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingTurretBullet : MonoBehaviour
{
    public float initialSpeed = 2f;
    public float acceleration = 0.5f;
    public float maxSpeed = 10f;
    public float homingStrength = 2f;
    public float damage = 10f;
    private Rigidbody2D rb;
    private GameObject player;
    private float timer;
    private bool hasHitEnemy = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        rb.linearVelocity = transform.up * initialSpeed;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 4f)
        {
            Destroy(gameObject);
        }

        if (player != null)
        {
            Vector2 direction = (Vector2)player.transform.position - rb.position;
            direction.Normalize();
            float rotateAmount = Vector3.Cross(direction, transform.up).z;
            rb.angularVelocity = -rotateAmount * homingStrength;
            rb.linearVelocity = transform.up * Mathf.Min(rb.linearVelocity.magnitude + acceleration * Time.deltaTime, maxSpeed);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitEnemy) return;

        if (other.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
    protected void OnTriggerStay2D(Collider2D _other)
    {
        if(_other.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            Attack();
        }
    }    
    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
        Destroy(gameObject);
    }
}
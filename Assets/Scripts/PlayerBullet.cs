using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float force;
    public float damage;
    private float timer;
    private bool hasHitEnemy;
    public GameObject bulletSprite; // Reference to the child GameObject containing the sprite

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * force * Time.deltaTime);
        timer += Time.deltaTime;

        if (timer > 1.5)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitEnemy) return;

        if (other.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
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
}
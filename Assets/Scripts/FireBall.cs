using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private float timer;
    [SerializeField] protected float damage;
    [SerializeField] private float force; // Ensure force is serialized

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Set the velocity based on the initial direction of the fireball
        rb.linearVelocity = transform.right * force;

        // Set the rotation of the fireball to face the direction it's moving
        float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 180));
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 0.3)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // Directly reduce the player's health without triggering invincibility
                player.Health -= Mathf.RoundToInt(damage);

                // Reset the last damage time and stop health regeneration coroutine
                player.ResetHealthRegenTimer();
            }
            Destroy(gameObject);
        }
    }
}
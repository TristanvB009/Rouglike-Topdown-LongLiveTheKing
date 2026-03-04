using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float damage;
    [SerializeField] protected float speed;
    protected Rigidbody2D rb;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
    }

    protected void OnTriggerStay2D(Collider2D _other)
    {
        PlayerController player = _other.GetComponent<PlayerController>();
        if (player != null)
        {
            if (player.pState == null)
            {

            }
            else if (player.pState.invincible)
            {

            }
            else
            {
                Attack(player);
            }
        }
    }

    protected virtual void Attack(PlayerController player)
    {
        player.TakeDamage(damage);
    }
}
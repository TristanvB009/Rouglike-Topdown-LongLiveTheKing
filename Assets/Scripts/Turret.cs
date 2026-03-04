using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using SpriteFlash;

public class Turret : Enemy
{
    public GameObject bullet;
    public Transform bulletPos;

    private float timer;
    private new GameObject player;
    private Animator animator;
    private SimpleFlash simpleFlash;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        simpleFlash = GetComponent<SimpleFlash>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        float distance = Vector2.Distance(transform.position, player.transform.position);
        timer += Time.deltaTime;

        if(timer > 2)
        {
            timer = 0;
            shoot();
        }

        if (player != null)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            RotateTowards(direction);
        }
    }
    void shoot()
    {
        Invoke("FireBullet" , 0.0f);
    }

    void FireBullet()
    {
        Instantiate(bullet, bulletPos.position, Quaternion.identity);
    }
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
        simpleFlash.Flash();
    }
    private void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}

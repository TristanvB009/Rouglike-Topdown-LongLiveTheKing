using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpriteFlash;

public class HomingTurret : Enemy
{
    public GameObject bullet;
    public Transform topBulletTransform;
    public Transform middleBulletTransform;
    public Transform bottomBulletTransform;
    private GameObject player;
    private Animator animator;
    private SimpleFlash simpleFlash;
    private float timer;

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
        if (PlayerController.Instance != null)
        {
            ChasePlayer();
        }
    }

void shoot()
{
    Invoke("FireBullets", 0.0f);
}

void FireBullets()
{
    // Fire the middle bullet at a -90-degree angle
    Quaternion middleBulletRotation = middleBulletTransform.rotation * Quaternion.Euler(0, 0, -90);
    Instantiate(bullet, middleBulletTransform.position, middleBulletRotation);

    // Fire the top bullet at a 50-degree angle
    Quaternion topBulletRotation = middleBulletTransform.rotation * Quaternion.Euler(0, 0, -130);
    Instantiate(bullet, topBulletTransform.position, topBulletRotation);

    // Fire the bottom bullet at a -50-degree angle
    Quaternion bottomBulletRotation = middleBulletTransform.rotation * Quaternion.Euler(0, 0, -50);
    Instantiate(bullet, bottomBulletTransform.position, bottomBulletRotation);
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
}

private void RotateTowards(Vector2 direction)
{
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
}
}
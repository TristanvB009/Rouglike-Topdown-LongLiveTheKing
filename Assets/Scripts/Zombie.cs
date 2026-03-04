using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpriteFlash;

public class Zombie : Enemy
{
    private SimpleFlash simpleFlash;

// Start is called before the first frame update
protected override void Start()
{
    base.Start(); // Call the base class Awake method to initialize 'rb'
    simpleFlash = GetComponent<SimpleFlash>();
}

// Update is called once per frame
protected override void Update()
{
    base.Update();
    if (PlayerController.Instance != null)
    {
        ChasePlayer();
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

public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
{
    base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    simpleFlash.Flash();
}
}
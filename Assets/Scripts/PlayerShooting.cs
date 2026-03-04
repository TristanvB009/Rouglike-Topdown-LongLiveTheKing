using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    public static PlayerShooting Instance;
    public GameObject bullet;
    public GameObject shootEffect;
    public Transform bulletTransform;
    public float timeBetweenShots = 0.15f;
    public GameObject normalGun;
    public GameObject machineGun;
    public float lastShootingAngle = 0; // Track the last shooting angle
    private bool canFire = true;
    private Animator anim;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryFireBullet();
        }
    }

    public void SetRotationAndFire(float angle)
    {
        lastShootingAngle = angle; // Update the last shooting angle
        if (angle == 0) // Right
        {
            bulletTransform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (angle == 180) // Left
        {
            bulletTransform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (angle == 45) // Right-Up
        {
            bulletTransform.rotation = Quaternion.Euler(0, 0, 45);
        }
        else if (angle == -45) // Right-Down
        {
            bulletTransform.rotation = Quaternion.Euler(0, 0, -45);
        }
        else if (angle == 135) // Left-Up
        {
            bulletTransform.rotation = Quaternion.Euler(0, 180, 45);
        }
        else if (angle == -135) // Left-Down
        {
            bulletTransform.rotation = Quaternion.Euler(0, 180, -45);
        }
        else
        {
            bulletTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        TryFireBullet();
    }

    public void ActivateMachineGun()
    {
        normalGun.SetActive(false); // Hide the normal gun
        machineGun.SetActive(true); // Unhide the machine gun
        Debug.Log("Machine gun activated!");
    }

    public void DeactivateMachineGun()
    {
        normalGun.SetActive(true); // Unhide the normal gun
        machineGun.SetActive(false); // Hide the machine gun
        Debug.Log("Machine gun deactivated!");
    }

    private void TryFireBullet()
    {
        if (canFire)
        {
            canFire = false;
            anim.SetBool("IsShooting", true); // Set IsShooting to true when the player starts shooting
            FireBullet();
            StartCoroutine(ResetFire());
        }
    }

    private void FireBullet()
    {
        if (bullet == null || shootEffect == null || bulletTransform == null)
        {
            return;
        }

        // Calculate the bullet position slightly downwards
        Vector3 bulletPosition = bulletTransform.position + new Vector3(0, -0.2f, 0);
        Instantiate(bullet, bulletPosition, bulletTransform.rotation);
        
        // Calculate the effect position a little bit ahead and slightly downwards of the bullet transform
        Vector3 effectPosition = bulletTransform.position + bulletTransform.right * 2.5f + new Vector3(0, -0.2f, 0);
        GameObject effect = Instantiate(shootEffect, effectPosition, bulletTransform.rotation);
        Destroy(effect, 0.1f);
    }

    private IEnumerator ResetFire()
    {
        yield return new WaitForSeconds(timeBetweenShots);
        canFire = true;
        anim.SetBool("IsShooting", false); // Set IsShooting to false when the player stops shooting
    }
}
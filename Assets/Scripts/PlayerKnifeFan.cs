using UnityEngine;
using UnityEngine.UI;

public class PlayerKnifeFan : MonoBehaviour
{
    public float timeBetweenFiring;
    public GameObject knifePrefab;
    public Image cooldownBar;
    public float knifeSpeed;
    public float returnSpeed;
    public Transform knifeTransform1;
    public Transform knifeTransform2;
    public Transform knifeTransform3;
    public Transform knifeTransform4;
    public Transform knifeTransform5;
    public Transform knifeTransform6;
    public Transform knifeTransform7;
    private float shotTimer = 0f;
    private bool canFire = true;
    private bool isReturning = false;
    private bool knivesOut = false;
    private GameObject[] knives;

    void Start()
    {
        InitializeCooldownBar();
    }

    void Update()
    {
        if (!canFire && !isReturning)
        {
            shotTimer -= Time.deltaTime;
            UpdateCooldownBar(1 - (shotTimer / timeBetweenFiring));
            if (shotTimer <= 0)
            {
                canFire = true;
                shotTimer = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && knives != null && KnivesExist() && !isReturning)
        {
            StartReturningKnives();
        }

        if (isReturning)
        {
            ReturnKnivesToPlayer();
        }

        if (Input.GetKeyDown(KeyCode.E) && canFire && !isReturning && !knivesOut)
        {
            TryFireKnives();
        }
    }

    private void TryFireKnives()
    {
        if (canFire && !knivesOut)
        {
            canFire = false;
            knivesOut = true; // Knives are now out
            FireKnives();
            UpdateCooldownBar(0); // Set the cooldown bar to empty
        }
    }

    private void FireKnives()
    {
        knives = new GameObject[7];
        float baseAngle = PlayerShooting.Instance.lastShootingAngle; // Get the last shooting angle

        // Fire knives using their respective transforms, adjusting direction based on baseAngle
        knives[0] = InstantiateKnife(knifeTransform1, baseAngle - 30);
        knives[1] = InstantiateKnife(knifeTransform2, baseAngle - 20);
        knives[2] = InstantiateKnife(knifeTransform3, baseAngle - 10);
        knives[3] = InstantiateKnife(knifeTransform4, baseAngle);
        knives[4] = InstantiateKnife(knifeTransform5, baseAngle + 10);
        knives[5] = InstantiateKnife(knifeTransform6, baseAngle + 20);
        knives[6] = InstantiateKnife(knifeTransform7, baseAngle + 30);
    }

    private GameObject InstantiateKnife(Transform knifeTransform, float angle)
    {
        // Adjust knife rotation based on the specified angle
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject knife = Instantiate(knifePrefab, knifeTransform.position, rotation);
        Rigidbody2D rb = knife.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = rotation * Vector3.right * knifeSpeed;
        }
        return knife;
    }

    private void StartReturningKnives()
    {
        isReturning = true;
        foreach (GameObject knife in knives)
        {
            if (knife != null)
            {
                knife.GetComponent<PlayerKnife>().StartReturning();
            }
        }
    }

    private void ReturnKnivesToPlayer()
    {
        if (AllKnivesDestroyed())
        {
            isReturning = false;
            knivesOut = false; // Knives are now returned
            StartCooldown();
        }
    }

    private bool AllKnivesDestroyed()
    {
        foreach (GameObject knife in knives)
        {
            if (knife != null)
            {
                return false;
            }
        }
        return true;
    }

    private bool KnivesExist()
    {
        foreach (GameObject knife in knives)
        {
            if (knife != null)
            {
                return true;
            }
        }
        return false;
    }

    private void StartCooldown()
    {
        shotTimer = timeBetweenFiring;
        canFire = false;
    }

    private void InitializeCooldownBar()
    {
        if (cooldownBar != null)
        {
            cooldownBar.fillAmount = 1; // Initialize the cooldown bar to full
        }
    }

    private void UpdateCooldownBar(float fillAmount)
    {
        if (cooldownBar != null)
        {
            cooldownBar.fillAmount = fillAmount; // Update the fill amount of the cooldown bar
        }
    }
}

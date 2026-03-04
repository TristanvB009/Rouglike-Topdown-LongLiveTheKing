using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SwordShootingScript : MonoBehaviour
{
    public GameObject swordPrefab;
    public float timeBetweenFiring;
    public Image cooldownBar;
    private float timer;
    private bool canFire = true;
    public Transform player;

    void Start()
    {
        if (cooldownBar != null)
        {
            cooldownBar.fillAmount = 1; // Initialize the cooldown bar to full
        }

        if (player == null)
        {
            if (PlayerController.Instance != null)
            {
                player = PlayerController.Instance.transform;
            }
            else
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }
        }
    }

    void Update()
    {
        if (!canFire)
        {
            timer += Time.deltaTime;
            if (cooldownBar != null)
            {
                cooldownBar.fillAmount = timer / timeBetweenFiring; // Update the fill amount based on the timer
            }
            if (timer > timeBetweenFiring)
            {
                canFire = true;
                timer = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && canFire)
        {
            FireSword();
        }
    }

    private void FireSword()
    {
        canFire = false;
        UpdateCooldownBar(0); // Set the cooldown bar to empty

        if (swordPrefab == null || player == null)
        {
            return;
        }

        GameObject sword = Instantiate(swordPrefab, player.position, Quaternion.identity);
        GalaxySwordAttack swordAttack = sword.GetComponent<GalaxySwordAttack>();
        if (swordAttack != null)
        {
            swordAttack.Init(player);
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
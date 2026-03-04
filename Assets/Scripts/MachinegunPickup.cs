using UnityEngine;

public class MachinegunPickup : MonoBehaviour
{
    // OnTriggerEnter2D is called when another collider enters the trigger collider attached to this object
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            // Perform the pickup action
            Pickup(other.gameObject);
        }
    }

    // Method to handle the pickup action
    private void Pickup(GameObject player)
    {
        // Change the player's weapon to the machine gun
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ActivateMachinegun();
        }

        // Destroy the pickup object
        Destroy(gameObject);
    }
}
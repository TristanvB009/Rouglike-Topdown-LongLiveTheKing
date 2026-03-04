using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    private Flamer flamer;

    private void Start()
    {
        // Find the Flamer component in the parent or ancestor GameObject
        flamer = GetComponentInParent<Flamer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet")) // Assuming the player's bullets have the tag "PlayerBullet"
        {
            flamer.WeakPointHit();
        }
    }
}
using UnityEngine;

public class Obsticle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collision with player detected!");
            // You can add additional actions here when a collision with the player occurs
        }
    }
}
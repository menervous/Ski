using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChalangeObs : MonoBehaviour
{
    public float slowdownFactor = 0.5f; // Adjust this value to control the slowdown effect

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collision with player detected!");
            SkierController skierController = other.GetComponent<SkierController>();
            if (skierController != null)
            {
                // Slow down the player
                skierController.ApplySlowdown(slowdownFactor);
            }
            // Remove the obstacle
            Destroy(gameObject);
        }
    }
}

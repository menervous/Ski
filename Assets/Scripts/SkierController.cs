using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class SkierController : MonoBehaviour
{
    public float turnSpeed = 5f; // Speed of turning
    public float maxTurnAngle = 90f; // Maximum turn angle
    public float diagonalTurnAngle = 45f; // Angle when pressing S and A/D
    public float movementSpeed = 10f; // Speed of forward movement
    public float speedBoostMultiplier = 2f; // Multiplier for speed boost
    public float boostDuration = 3f; // Duration of the speed boost in seconds
    public LayerMask groundLayer; // Layer mask for the ground
    public Animator animator; // Animator component reference
    public GameObject obstaclePrefab; // Reference to the obstacle prefab
    public AudioClip knockbackSound; // Sound to play when knocked back

    private Rigidbody rb;
    private float currentTurnAngle = 0f;
    private bool isBoosting = false;
    private bool isKnockedBack = false;

    private float originalSpeed; // Store the original speed before applying slowdown
    private bool isSlowedDown = false; // Flag to track if the skier is slowed down
    private float slowdownFactor = 0.5f; // Factor by which the skier's speed is reduced
    private float slowdownTimer = 0f; // Timer to keep track of slowdown duration

    public UnityEvent OnPlayerKnockback; // Event to handle player knockback

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Initially rotate the character 180 degrees to face the opposite direction
        transform.rotation = Quaternion.Euler(0, 180, 0);
        currentTurnAngle = 180f; // Initialize currentTurnAngle to 180 degrees

        originalSpeed = movementSpeed; // Store the original speed

        // Subscribe to the event
        OnPlayerKnockback.AddListener(HandlePlayerKnockback);
    }

    void FixedUpdate()
    {
        HandleMovement();
        UpdateSlowdown();
    }

    void HandleMovement()
    {
        // Check if the player is grounded
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f, groundLayer);

        // Get input for turning
        float horizontalInput = -Input.GetAxis("Horizontal"); // Invert the input axis
        float verticalInput = Input.GetAxis("Vertical");

        float turnInput = 0f;

        // If 'S' is pressed along with 'A' or 'D', set the turn input to the appropriate diagonal turn angle
        if (verticalInput < 0 && horizontalInput != 0)
        {
            turnInput = horizontalInput > 0 ? diagonalTurnAngle : -diagonalTurnAngle;
        }
        else if (verticalInput == 0 && horizontalInput != 0)
        {
            // If 'A' or 'D' is pressed alone, set the turn input to 90 degrees
            turnInput = horizontalInput > 0 ? maxTurnAngle : -maxTurnAngle;
        }
        // Calculate the desired turn angle based on input
        float desiredTurnAngle = 180f + turnInput;

        // Smoothly interpolate to the desired turn angle if grounded
        if (isGrounded)
        {
            currentTurnAngle = Mathf.Lerp(currentTurnAngle, desiredTurnAngle, Time.deltaTime * turnSpeed);

            // Clamp the current turn angle to ensure it doesn't exceed the max turn angle
            currentTurnAngle = Mathf.Clamp(currentTurnAngle, 180f - maxTurnAngle, 180f + maxTurnAngle);
        }

        // Calculate the speed based on the turn angle
        float currentSpeed = Mathf.Lerp(movementSpeed, 0f, Mathf.Abs((currentTurnAngle - 180f) / maxTurnAngle));

        // Apply the turn angle to the rotation
        Quaternion turnRotation = Quaternion.Euler(0, currentTurnAngle, 0);
        rb.MoveRotation(turnRotation);

        // Apply forward movement while keeping gravity intact if grounded
        Vector3 forwardMovement = transform.forward * currentSpeed;
        Vector3 newVelocity = forwardMovement;
        newVelocity.y = rb.velocity.y; // Preserve the y velocity to maintain gravity effect

        if (isGrounded)
        {
            rb.velocity = newVelocity;
        }

        // Update the animator with the current speed
        animator.SetFloat("Speed", currentSpeed);

        // Check for speed boost activation if grounded
        if (Input.GetKeyDown(KeyCode.Space) && !isBoosting && isGrounded) // Changed from KeyCode.E to KeyCode.Space
        {
            StartCoroutine(SpeedBoost());
        }
    }

    IEnumerator SpeedBoost()
    {
        isBoosting = true;
        movementSpeed *= speedBoostMultiplier; // Increase movement speed
        yield return new WaitForSeconds(boostDuration);
        movementSpeed /= speedBoostMultiplier; // Reset movement speed
        isBoosting = false;
    }

    void UpdateSlowdown()
    {
        // If the skier is slowed down, update the slowdown effect
        if (isSlowedDown)
        {
            slowdownTimer += Time.deltaTime;

            // Calculate the percentage of the slowdown duration completed
            float slowdownPercentage = slowdownTimer / boostDuration;

            // Gradually restore the speed back to its original value over the slowdown duration
            movementSpeed = Mathf.Lerp(movementSpeed * slowdownFactor, originalSpeed, slowdownPercentage);

            // If the slowdown duration is complete, reset the slowdown
            if (slowdownTimer >= boostDuration)
            {
                isSlowedDown = false;
                slowdownTimer = 0f;
                movementSpeed = originalSpeed;
            }
        }
    }

    public void ApplySlowdown(float factor)
    {
        // Slow down the skier
        isSlowedDown = true;
        slowdownTimer = 0f;
        slowdownFactor = factor; // Set the slowdown factor
    }

    // Method to spawn an obstacle
    public void SpawnObstacle(Vector3 position)
    {
        Instantiate(obstaclePrefab, position, Quaternion.identity);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            // Trigger the knockback event
            OnPlayerKnockback.Invoke();
        }
    }

    void HandlePlayerKnockback()
    {
        // Play knockback sound
        if (knockbackSound != null)
        {
            AudioSource.PlayClipAtPoint(knockbackSound, transform.position);
        }

        // Start knockback coroutine
        StartCoroutine(Knockback());
    }

    IEnumerator Knockback()
    {
        isKnockedBack = true;
        // Disable player control
        // You can disable player controls by setting input controls to zero or by disabling the entire script/component.
        // For simplicity, let's just stop the movement.
        movementSpeed = 0;

        // Wait for knockback duration
        yield return new WaitForSeconds(1.5f); // Adjust the duration as needed

        // Re-enable player control
        isKnockedBack = false;
        movementSpeed = originalSpeed;
    }
}
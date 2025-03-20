using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;            // Max move speed
    public float groundDrag = 5f;           // Drag While Grounded
    public float jumpForce = 12f;           // Force applied when Jumping
    public float jumpCooldown = 0.25f;      // jumpCooldown in case we want it
    public float airMultiplier = 0.4f;      // Speed multiplier for air movement for fluid momentum
    public bool readyToJump;                // Flag used for jumpCooldown


    [Header("Ground Check")]
    public Transform groundCheck;           // Empty object attached to player to check for the ground beneath them
    public float groundDistance = 0.2f;     // Radius around groundCheck to check if the player is on the ground
    public LayerMask whatIsGround;          // Layer assigned to ground objects to reset jumps and apply ground drag or airMultiplier
    public bool grounded;                   // Grounded flag


    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space; // Area for Keybinds so we can apply settings from a settings menu



    public Transform orientation;           // Empty object attached to player to get their rotation for movement application based on rotation
    private Rigidbody rigidBody;            // Player's rigid body component
    private float horizontalInput;          // Track horizontal key presses (A and D)
    private float verticalInput;            // Track vertical key presses (W and S)

    private Vector3 moveDirection;          // Vector to calculate how to apply force


    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();  // Get Player's Rigidbody component
        rigidBody.freezeRotation = true;        // Freeze RigidBody's rotation
        readyToJump = true;                     // Allow the player to start with jump ready
    }


    private void Update() {
        // Checks in a sphere around the groundCheck empty object for objects with whatIsGround layer title to detect if the player is grounded
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

        // Gets the user's input
        MyInput();
        // Caps max speed so the player doesnt infinitely accelerate
        SpeedControl();

        // Applies ground drag if grounded
        if(grounded)
        {
            rigidBody.drag = groundDrag;
        }
        // Doesn't apply ground drag if airborne
        else
        {
            rigidBody.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        // Moves player automatically encorporating Time.deltaTime
        MovePlayer();
    }

    // Function that tracks user input
    private void MyInput()
    {
        // Gets horizontal and vertical input (WASD)
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Detects Space key press. Allows jump as long as jumpcooldown has expired and is grounded
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            // jump flag
            readyToJump = false;
            // Jump function
            Jump();
            // Starts jump cooldown
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    // Function that moves the player
    private void MovePlayer()
    {
        // Creates vector to apply force based on the player's orientation and their directional input
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(grounded)
        {
            // Applies grounded acceleration
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            // Applies acceleration with airMultiplier
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        
    }

    // Function that prevents infinite acceleration
    private void SpeedControl()
    {
        // Gets the velocity (excluding the y axis)
        Vector3 flatVelocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);

        // Checks if magnitude of x and y velocity is over the move speed
        if (flatVelocity.magnitude > moveSpeed)
        {
            // Normalizes the flat velocity (sets flat velocity to 1) and multiplies by move speed to cap the horizontal move speed
            Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;

            // Sets rigidBody's velocity to this limited velocity and keeps y velocity
            rigidBody.velocity = new Vector3(limitedVelocity.x, rigidBody.velocity.y, limitedVelocity.z);
        }
    }

    // Function that applies force upward for jump
    private void Jump()
    {
        // Resets the y velocity so jump velocity is consistant (not added to preexisting vertical velocity positive or negative)
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);

        // Adds force upward multiplied by the jump force
        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    // Function that resets readyToJump for Invoke call on jump cooldown
    private void ResetJump()
    {
        readyToJump = true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class PlayerMovementManager : MonoBehaviour
{
    [Header("Initialization")]
    public Transform orientation;                           // Reference to the player's orientation
    public Rigidbody playerRigidBody;                       // Reference to the player's rigid body
    private Transform body;                                 // Gets the body child object in the start function
    private Renderer bodyRenderer;                          // Gets the renderer of the body in the start function
    [HideInInspector] public float bodyLength;              // Gets the length of the body after rendering from the renderer
    [HideInInspector] public float bodyWidth;               // Gets the width of the body after rendering from the renderer
    [HideInInspector] public float bodyHeight;              // Gets the height of the body after rendering from the renderer

    [Header("Movement Scripts")]
    public GroundedMovementScript gMs;                      // Reference to the grounded movement script
    public AirborneMovementScript aMs;                      // Reference to the airborne movement script

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;                 // Jump keybind
    public KeyCode sprintKey = KeyCode.LeftShift;           // Sprint Keybind
    public KeyCode crouchKey = KeyCode.C;                   // Crouch Keybind
    public KeyCode slideKey = KeyCode.LeftControl;          // Slide Keybind

    [Header("Speed Caps")]
    public float groundedDrag;                              // Drag applied when grounded
    public float crouchSpeed;                               // Crouch top speed
    public float walkSpeed;                                 // Walk top speed
    public float sprintSpeed;                               // Sprint top speed
    public float slideSpeed;                                // Slide top speed
    public float airStraifeSpeed;                           // Ariborne top speed
    public float wallRunSpeed;                              // Wall run top speed
    public float wallClimbUpSpeed;                          // Wall climb up speed
    public float wallClimbSideSpeed;                        // Wall climbing side straife speed

    [Header("Slope Speed Modifiers")]
    public float speedIncreaseMultiplier;                   // Slide speed increase on slope
    public float slopedIncreaseMultipler;                   // Slope angle speed increase while sliding
    private Coroutine speedLerpCoroutine;                   // Coroutine to lerp player speed for large changes in speed

    [Header("Movement State")]
    public MovementState movementState;                     // Enum to track the player's movement state
    public enum MovementState
    {
        crouching,
        walking,
        sprinting,
        sliding,
        wallRunning,
        wallClimbing,
        airborne
    }

    [HideInInspector] public float moveSpeed;               // Player's current movement speed
    [HideInInspector] public float desiredMoveSpeed;        // Player's top speed for their state
    [HideInInspector] public float lastDesiredMoveSpeed;    // Player's top speed on the last frame

    [Header("Ground Check")]
    public LayerMask whatIsGround;                          // Layer mask for ground checks
    [HideInInspector] public bool grounded;                 // Flag for if the player is grounded

    [Header("UI References")]
    public TextMeshProUGUI topRightText1;                   // UI text element reference
    public TextMeshProUGUI topRightText2;                   // UI text element reference
    public TextMeshProUGUI bottomLeftText;                  // UI text element reference


    [HideInInspector] public float horizontalInput;         // Variable for tracking Horizontal input
    [HideInInspector] public float verticalInput;           // Variable for tracking Vertical input
    [HideInInspector] public Vector3 moveDirection;         // Variable for the player's current inputted movement direction



    private void Start()
    {
        // Freeze rigidbody rotation
        playerRigidBody.freezeRotation = true;

        // Get the child from position 0 (the body object)
        body = transform.GetChild(0);
        if (body != null)
        {
            // Get the renderer component of that object
            bodyRenderer = body.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                // Get the Length, Width, and Height of the body from the renderer
                bodyWidth = bodyRenderer.bounds.size.x;
                bodyLength = bodyRenderer.bounds.size.z;
                bodyHeight = bodyRenderer.bounds.size.y;
            }
            else
            {
                // Default with the capsule object if the code grabbed the wrong component or the body doesnt have a renderer
                bodyWidth = 1;
                bodyLength = 1;
                bodyHeight = 2;
            }
        }
    }

    private void Update()
    {
        // Shoot a raycast for ground check
        grounded = Physics.Raycast(orientation.position,        // Origin of the ray
                                 Vector3.down,                  // Ray direction (down)
                                 (bodyHeight * 0.5f) + 0.2f,    // Length of the ray
                                 whatIsGround)                  // Layer mask of what to look for
                        || gMs.OnSlope();           // Slope check also activates the grounded flag

        

        // Gets input and calls movement activation functions
        GetInput();
        // Changes player's state
        StateManager();
        // Normalizes rigidbody velocity and multiplies by move speed, lerping if the change in speed is too large
        SpeedControl();
        // Updates the ui values
        UpdateUI();
    }

    private void FixedUpdate()
    {
        // Adds force to the rigid body
        ManageMovement();
        
    }

    /*
     * 
     * ---------- Input Retrieval ---------- (Update)
     * 
     */

    private void GetInput()
    {
        // Get movement input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Get grounded or airborne input
        if (grounded)
        {
            gMs.GetGroundedInput();

        }
        else
        {
            aMs.GetAirborneInput();

        }
    }

    /*
     * 
     * ---------- State Management ---------- (Update)
     * 
     */

    private void StateManager()
    {
        // Call grounded and airborne state handlers
        if (grounded)
        {
            gMs.groundedStateHandler();
        }
        else
        {
            aMs.airborneStateHandler();
        }
    }

    /*
     * 
     * ---------- Movement Management ---------- (Fixed Update)
     * 
     */

    private void ManageMovement()
    {
        // Set their moveDirection based on their input
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Set their gravity based on whether they are on a slope or wall running
        playerRigidBody.useGravity = (!gMs.OnSlope() && !aMs.wRs.isWallRunning);

        // Call wall check for climbing script (in player movement manager since grounded and airborne use wall climbing)
        gMs.wCs.WallCheck();

        // Call the movement handlers to add force
        if (grounded)
        {
            gMs.handleGroundedMovement();

        }
        else
        {
            aMs.handleAirborneMovement();
        }
    }

    /*
     * 
     * ---------- Speed Control ---------- (Update)
     * 
     */

    public void SpeedControl()
    {
        // If grounded, add drag, if not let physics engine handle gravity
        if (grounded)
        {
            playerRigidBody.drag = groundedDrag;
        }
        else
        {
            playerRigidBody.drag = 0f;
        }


        // * Lerping Player Movement if needed *

        // Lerp move speed if the player's moveSpeed - 1f is above the sprint speed or their desired move speed is above the sprint speed
        if (desiredMoveSpeed > sprintSpeed || moveSpeed - 1f > sprintSpeed)
        {
            // If no coroutine exists, start a lerp speed coroutine
            if (speedLerpCoroutine == null)
            {
                speedLerpCoroutine = StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            // If the desired move speed changed since the last frame, reset the coroutine
            else if (desiredMoveSpeed != lastDesiredMoveSpeed)
            {
                StopCoroutine(speedLerpCoroutine);
                speedLerpCoroutine = StartCoroutine(SmoothlyLerpMoveSpeed());
            }
        }
        // Set move speed immediately if their move speed is below the sprint speed (for responsive movement changes)
        else
        {
            // If there is a coroutine running, stop it
            if (speedLerpCoroutine != null)
            {
                StopCoroutine(speedLerpCoroutine);
                speedLerpCoroutine = null;
            }
            // Set the move speed directly
            moveSpeed = desiredMoveSpeed;
        }

        // Log the desired move speed for this frame to detect change in the next frame
        lastDesiredMoveSpeed = desiredMoveSpeed;


        // * Speed Cap *
        // If on slope
        if (gMs.OnSlope() && !gMs.jumpOnSlope)
        {
            // If their total velocity is above the moveSpeed
            if (playerRigidBody.velocity.magnitude > moveSpeed)
            {
                // Adjust their velocity to be the moveSpeed
                playerRigidBody.velocity = playerRigidBody.velocity.normalized * moveSpeed;
            }
        }
        else 
        {
            // If not on a slope, ignore the y velocity
            Vector3 flatVelocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);

            // If going above the move speed, manually cap the movement speed ignoring the y velocity
            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                playerRigidBody.velocity = new Vector3(limitedVelocity.x, playerRigidBody.velocity.y, limitedVelocity.z);
            }
        }


    }

    // Coroutine to lerp move speed increase or decrease over time
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;                                                 // Timer to dynamically adjust move speed
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);     // Difference in move speed
        float startValue = moveSpeed;                                   // Starting move speed to lerp from

        // Loop based on time
        while (time < difference)
        {
            // Lerp the movement speed as a factor of time divided by the difference between the move speeds
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);


            if (gMs.OnSlope())
            {
                // If on a slope, calculate speed boost for sliding based on slope angle
                float slopeAngle = Vector3.Angle(Vector3.up, gMs.slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                // Adjust timer more quickly for sliding down slopes to lerp more quickly
                time += Time.deltaTime * speedIncreaseMultiplier * slopedIncreaseMultipler * slopeAngleIncrease;
            }
            else
            {
                // Standard lerp speed based on time
                time += Time.deltaTime * speedIncreaseMultiplier;
            }

            yield return null;
        }
        // Set lerp coroutine to null if the lerp has ended naturally
        speedLerpCoroutine = null;
    }

    /*
     * 
     * ---------- Update UI values ---------- (Update)
     * 
     */

    private void UpdateUI()
    {
        double flatSpeed = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z).magnitude;
        double totalSpeed = playerRigidBody.velocity.magnitude;

        //topRightText1.text = "Horizontal Speed: " + Math.Round(flatSpeed, 2).ToString();
        //topRightText2.text = "Total Speed: " + totalSpeed.ToString();

        topRightText1.text = "Last Desired Move Speed:  " + lastDesiredMoveSpeed.ToString();
        topRightText2.text = "Move Speed:" + moveSpeed.ToString();

        bottomLeftText.text = "Desired Move Speed: " + desiredMoveSpeed.ToString();
    }

}

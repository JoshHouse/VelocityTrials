using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerMovementManager : MonoBehaviour
{
    [Header("Initialization")]
    public Transform orientation;                           // Reference to the player's orientation
    public Rigidbody playerRigidBody;                       // Reference to the player's rigid body
    public AnimationManager animManager;
    public Transform playerModel;
    private Transform body;                                 // Gets the body child object in the start function
    private Renderer bodyRenderer;                          // Gets the renderer of the body in the start function
    [HideInInspector] public float bodyLength;              // Gets the length of the body after rendering from the renderer
    [HideInInspector] public float bodyWidth;               // Gets the width of the body after rendering from the renderer
    [HideInInspector] public float bodyHeight;              // Gets the height of the body after rendering from the renderer

    [HideInInspector] public bool firstPerson;

    [Header("Movement Scripts")]
    public GroundedMovementScript gMs;                      // Reference to the grounded movement script
    public AirborneMovementScript aMs;                      // Reference to the airborne movement script

    [Header("Always Accessable Movement Mechanic Scripts")]
    public GrapplingScript gS;
    public WallClimbingScript wCs;
    public MantlingScript mS;

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
        idle,
        crouching,
        walking,
        sprinting,
        sliding,
        mantling,
        wallRunning,
        wallClimbing,
        grappleSwinging,
        grapplePulling,
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

        firstPerson = true;

        // Get the child from position 0 (the body object)
        body = transform.GetChild(0);
        if (body == null)
        {
            // Default with the capsule object if the code grabbed the wrong component
            bodyWidth = 1;
            bodyLength = 1;
            bodyHeight = 2;
            return;
        }

        // Get the renderer component of that object
        bodyRenderer = body.GetComponent<Renderer>();
        if (bodyRenderer == null)
        {
            // Default with the capsule object if the code couldnt find the renderer
            bodyWidth = 1;
            bodyLength = 1;
            bodyHeight = 2;
            return;
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

        // Get the Length, Width, and Height of the body from the renderer
        if (body == null || bodyRenderer == null)
        {
            bodyWidth = 1;
            bodyLength = 1;
            bodyHeight = 2;
        }
        else
        {
            bodyWidth = bodyRenderer.bounds.size.x;
            bodyLength = bodyRenderer.bounds.size.z;
            bodyHeight = bodyRenderer.bounds.size.y;
        }


        // Gets input and calls movement activation functions
        GetInput();

        // Changes player's state
        StateManager();

        // Rotates the players arm while grappling
        gS.RotateArmOnGrapple();

        animManager.checkForPlayingAnimation();

        // Updates the ui values
        UpdateUI();

        if(mS.isMantling)
        {
            if (speedLerpCoroutine != null)
            {
                StopCoroutine(speedLerpCoroutine);
                speedLerpCoroutine = null;
            }
            return;
        }

        // Allow spring joint on grapple to handle physics during grapple. 
        if (gS.isSwingGrappling || gS.isPullGrappling)
        {
            if (speedLerpCoroutine != null)
            {
                StopCoroutine(speedLerpCoroutine);
                speedLerpCoroutine = null;
            }

            if (gS.isSwingGrappling)
            {
                animManager.PlayAnim("SwingGrapple");
            }
            else
            {
                animManager.PlayAnim("PullGrapple");
            }


            // Speed control doesn't run when grappling. Updates the move speed based on the player's velocity
            // To lerp back down when they stop the grapple
            float flatSpeed = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z).magnitude;
            moveSpeed = flatSpeed;
            return;
        }

        // Normalizes rigidbody velocity and multiplies by move speed, lerping if the change in speed is too large
        SpeedControl();
    }

    private void FixedUpdate()
    {
        // Adds force to the rigid body
        ManageMovement();
        
    }

    private void LateUpdate()
    {
        // Draw's the grapple rope after all values have been updated
        gS.DrawGrappleRope();
    }

    /*
     * 
     * ---------- Input Retrieval ---------- (Update)
     * 
     */

    private void GetInput()
    {
        // Get movement input
        horizontalInput = GameManager.instance.moveInput.x;
        verticalInput = GameManager.instance.moveInput.y;

        if (Input.GetKeyDown(KeyCode.Tab))
            firstPerson = !firstPerson;

        bool shouldGatherStandardInput = attemptGatherMechanicInput();

        // Return if mechanic movement input was gathered
        if (!shouldGatherStandardInput)
            return;

        // Get grounded or airborne input if not using general movement mechanics
        if (grounded)
        {
            gS.grappleCount = 0;
            gMs.GetGroundedInput();
            return;
        }

        aMs.GetAirborneInput();
    }

    private bool attemptGatherMechanicInput()
    {
        if (noMovementMechanicsRunning())
        {
            if (lookForStartInputs())
                return false;

            return true;
        }

        gS.predictionVisualizer.SetActive(false);

        if (gS.isPullGrappling)
            return false;

        if (gS.isSwingGrappling)
        {
            // If they are grappling and they release the grapple key
            if (GameManager.instance.gSwingReleased && gS.isSwingGrappling)
            {
                // Stop the Grapple
                gS.EndSwingGrapple();

                return false;
            }
        }

        if (mS.isMantling)
            return false;


        // If they are climbing and they stop holding into the wall
        if (verticalInput <= 0)
        {
            // Stop Climbing
            wCs.StopClimbing();

            return false;
        }

        // If they press the jump key while climbing
        if (GameManager.instance.jumpPressed)
        {
            // Stop climbing and wall jump
            wCs.StopClimbing();
            wCs.WallJump();

            return false;
        }

        return false;
    }

    private bool lookForStartInputs()
    {
        if (gS.CanGrapple() && GameManager.instance.gPullPressed)
        {
            gS.StartPullGrapple();

            return true;
        }

        // If not already grappling, can grapple, and they press the grapple key
        if (gS.CanGrapple() && GameManager.instance.gSwingPressed)
        {
            // Start grapple
            gS.StartSwingGrapple();

            return true;
        }

        if (GameManager.instance.jumpPressed && mS.canMantle())
        {
            mS.startMantle();
            return true;
        }

        // If they can climb and they press the jump key and they aren't grappling
        if (GameManager.instance.jumpPressed && wCs.CanClimb())
        {
            // Start climbing
            wCs.StartClimbing();

            return true;
        }

        return false;
    }

    private bool noMovementMechanicsRunning()
    {
        return !wCs.isClimbing && !mS.isMantling && !gS.isSwingGrappling && !gS.isPullGrappling;
    }

    /*
     * 
     * ---------- State Management ---------- (Update)
     * 
     */

    private void StateManager()
    {
        if (gS.isPullGrappling)
        {
            gS.predictionVisualizer.SetActive(false);
            movementState = MovementState.grapplePulling;
            return;
        }

        // If Grappling
        if (gS.isSwingGrappling)
        {
            gS.predictionVisualizer.SetActive(false);
            // Set their state
            movementState = MovementState.grappleSwinging;
            return;
        }

        if (mS.isMantling)
        {
            gS.predictionVisualizer.SetActive(false);
            movementState = MovementState.mantling;
            return;
        }

        // If they are climbing
        if (wCs.isClimbing)
        {
            gS.predictionVisualizer.SetActive(false);
            // Set their state
            movementState = MovementState.wallClimbing;

            // Set Desired Move Speed to wallClimbSideSpeed
            desiredMoveSpeed = wallClimbSideSpeed;
            return;
        }

        // Call grounded and airborne state handlers if not using generalized movement mechanics
        if (grounded)
        {
            gMs.groundedStateHandler();
            return;
        }

        aMs.airborneStateHandler();
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
        wCs.WallCheck();
        if (gS.isPullGrappling)
            return;

        // let spring joint in grappling script handle player movement
        if (gS.isSwingGrappling)
            return;

        if (mS.isMantling)
        {
            mS.handleMantle();
            return;
        }
            

        // If they are climbing then call climbing script's movement handler
        if (wCs.isClimbing)
            wCs.ClimbingMovement();
        

        // Call the movement handlers to add force
        if (grounded)
        {
            gMs.handleGroundedMovement();
            return;
        }

        aMs.handleAirborneMovement();
    }

    /*
     * 
     * ---------- Speed Control ---------- (Update)
     * 
     */

    public void SpeedControl()
    {
        playerRigidBody.drag = (grounded ? groundedDrag : 0f);

        // Lerping speed
        LerpIfNeeded();

        // Log the desired move speed for this frame to detect change in the next frame
        lastDesiredMoveSpeed = desiredMoveSpeed;


        // If on slope
        if (gMs.OnSlope() && !gMs.jumpOnSlope)
        {
            // If their total velocity is not above the moveSpeed, return
            if (playerRigidBody.velocity.magnitude <= moveSpeed)
                return;

            // Adjust their velocity to be the moveSpeed
            playerRigidBody.velocity = playerRigidBody.velocity.normalized * moveSpeed;
            return;
        }


        // If not on a slope, ignore the y velocity
        Vector3 flatVelocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);

        // If not going above the move speed ignoring y velocity, return
        if (flatVelocity.magnitude <= moveSpeed)
            return;


        // If going above the move speed, manually cap the movement speed ignoring the y velocity
        Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
        playerRigidBody.velocity = new Vector3(limitedVelocity.x, playerRigidBody.velocity.y, limitedVelocity.z);

    }

    private void LerpIfNeeded()
    {
        // Set move speed immediately if their move speed and desired speed is less than or equal to the sprint speed
        if (desiredMoveSpeed <= sprintSpeed && moveSpeed - 1f <= sprintSpeed)
        {
            // If there is a coroutine running, stop it
            if (speedLerpCoroutine != null)
            {
                StopCoroutine(speedLerpCoroutine);
                speedLerpCoroutine = null;
            }

            // Set the move speed directly
            moveSpeed = desiredMoveSpeed;
            return;
        }

        // If no coroutine exists, start a lerp speed coroutine
        if (speedLerpCoroutine == null)
        {
            speedLerpCoroutine = StartCoroutine(SmoothlyLerpMoveSpeed());
            return;
        }

        // If the desired move speed changed since the last frame, reset the coroutine
        if (desiredMoveSpeed != lastDesiredMoveSpeed)
        {
            StopCoroutine(speedLerpCoroutine);
            speedLerpCoroutine = StartCoroutine(SmoothlyLerpMoveSpeed());
            return;
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

            // Stops the function until the next frame. Will pick up from here on the next frame
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

        topRightText1.text = "Movement State:  " + movementState.ToString();
        topRightText2.text = "Move Speed: " + moveSpeed.ToString();

        bottomLeftText.text = "OnSlope: " + gMs.OnSlope().ToString();
    }

}

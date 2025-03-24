using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Initialization")]
    public Transform orientation;           // Empty object attached to player to get their rotation for movement application based on rotation
    public MovementState movementState;     // Current state of the player based on enumeration below
    public enum MovementState               // Enumeration to track the current state of the player
    {
        walking,
        sprinting,
        crouching,
        airborne,
        grappling
    }

    private Rigidbody rigidBody;            // Player's rigid body component
    private float horizontalInput;          // Track horizontal key presses (A and D)
    private float verticalInput;            // Track vertical key presses (W and S)
    private Vector3 moveDirection;          // Vector to calculate how to apply force
    private Transform body;                 // Gets the body child object in the start function
    private Renderer bodyRenderer;          // Gets the renderer of the body in the start function
    private float bodyLength;               // Gets the length of the body after rendering from the renderer
    private float bodyWidth;                // Gets the width of the body after rendering from the renderer
    private float bodyHeight;               // Gets the height of the body after rendering from the renderer


    [Header("Movement")]
    private float moveSpeed;                // Max movement speed changed dynamically based on player's movement state
    public float walkSpeed = 7f;            // Max walking speed
    public float sprintSpeed = 10f;         // Max Sprinting speed
    public float groundDrag = 5f;           // Drag While Grounded


    [Header("Jumping")]
    public float jumpForce = 12f;           // Force applied when Jumping
    public float jumpCooldown = 0.25f;      // jumpCooldown in case we want it
    public float airMultiplier = 0.4f;      // Speed multiplier for air movement for fluid momentum
    private bool readyToJump;               // Flag used for jumpCooldown
    private bool doubleJumpReady;           // Flag to allow for double jumping


    [Header("Crouching")]
    public float crouchSpeed = 3.5f;        // Movement speed while crouching
    public float crouchYscale = 0.5f;       // Amount the player shrinks when crouching
    private float startYscale;              // Start scale to reset the player after crouching

    [Header("Grappling")]
    public LayerMask whatIsGrapplable;      // Layer mask for what is grapplable
    public Transform grappleFirePoint;      // Reference to the transform component of the fire point on the grapple arm
    public Transform playerCamera;          // Reference to the playerCamera's transform component
    public Transform armGrapple;            // Reference to the grapple arm's transform component
    public int grapplesAllowed = 2;         // Amount of allowed grapples before touching the ground again
    public float maxGrappleDistance = 100f; // Max distance you can grapple
    public float rotationSpeed = 5f;        // Rotation speed of the arm when grappling

    private LineRenderer lineRenderer;      // Reference to the line renderer of the grapple arm
    private Vector3 grapplePoint;           // Vector3 of the point you are grappling to
    private RaycastHit grappleHit;          // Variable to store the raycast response
    private GameObject grappleHitObject;    // Stores the object you hit with the raycast
    private SpringJoint grappleJoint;       // SpringJoint added to the player for grapple mechanics
    private float distanceFromPoint;        // Distance from fire point to the grapple location
    private Quaternion desiredRotation;     // Variable to store what the desired rotation of the arm should be
    private bool isGrappling;               // Flag for whether the player is grappling
    private int grappleCount;

    [Header("Ground Check")]
    public Transform groundCheck;           // Empty object attached to player to check for the ground beneath them
    public float groundDistance = 0.2f;     // Radius around groundCheck to check if the player is on the ground
    public LayerMask whatIsGround;          // Layer assigned to ground objects to reset jumps and apply ground drag or airMultiplier
    private Vector3 groundCheckArea;        // Vector3 for the area to check for ground under the player (calculated based on size)
    private bool grounded;                  // Grounded flag


    [Header("Slope Handling")]
    public float maxSlopeAngle = 40f;       // Maximum slope the player can walk up
    private RaycastHit slopeHit;            // Detected slope that the player hit
    private bool jumpOnSlope;               // Flag Variable used to allow jumping on ramps
    private bool enteredSlope;             // Flag to detect when the player is entering a slope
    private bool exitingSlope;              // Flag to detect when the player is exiting a slope


    // Area for Keybinds so we can apply settings from a settings menu
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;             // Space keybind
    public KeyCode sprintKey = KeyCode.LeftShift;       // Sprint keybind
    public KeyCode crouchKey = KeyCode.LeftControl;     // Crouch keybind
    public KeyCode grappleKey = KeyCode.Mouse0;         // Grapple keybind



    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();                  // Get Player's Rigidbody component
        rigidBody.freezeRotation = true;                        // Freeze RigidBody's rotation
        lineRenderer = armGrapple.GetComponent<LineRenderer>(); // Get the line renderer for rope rendering when grappling

        isGrappling = false;                    // Initialize isGrappling to false when the game starts
        grappleCount = 0;
        readyToJump = true;                     // Allow the player to start with jump ready
        enteredSlope = false;

        startYscale = transform.localScale.y;   // Grab the initial scale of the player to reset after crouching

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

            /*
             * Vector 3 of the length and width of the body and a height of the ground distance:
             * This is used to initialize the groundCheck boolean. It is essentially the area we will be checking.
             * All distances start at the origin of the groundcheck object, which is in the center of the player.
             * so we devide by two to get the distance to the edge from the center. We subtract a small amount to
             * avoid collision detection issues when pushed up against a ground object.the groundCheck object is at
             * the player's feet so we don't need to divide it by two since the distance from the origin starts at
             * the player's feet.
             * 
             * So what this says is, "I want you to check a box under the player that is the length of the player and 
             * the width ofthe player, but the height specified in the unity editor for how far we want to check under the player"
             */
            groundCheckArea = new Vector3(bodyWidth / 2 - .01f, groundDistance, bodyLength / 2 - .01f);
        }
    }


    private void Update() {
        // Checks in a box around the groundCheck empty object for objects with whatIsGround layer title
        // to detect if the player is grounded
        grounded = Physics.CheckBox(groundCheck.position, groundCheckArea, Quaternion.identity, whatIsGround);
        // joint is destroyed when you end grapple so if joint == null, grappling = false
        isGrappling = grappleJoint != null;

        /*
         * Order Reasoning:
         * - UserInput(): 
         *      - Jump does not allow crouching size change. When Jump is called, it resets the player's size but not their speed
         * - StateHandler():
         *      - Based on the User's state, it sets the users max speed based on crouching, walking, or sprinting
         *      - Handling is Different based on grounded or airborne. Max Speed Stays the same but crouch sizing is different
         * - SpeedControl():
         *      - Encorporates the player's max speed and prevents infinite acceleration
         */
        // Gets the user's input
        UserInput();
        // Changes player's movement state based on their actions performed
        StateHandler();
        // Rotates the arm when grappling
        RotateArmOnGrapple();
        

        // Applies ground drag if grounded
        if(grounded)
        {
            rigidBody.drag = groundDrag;
        }
        // Doesn't apply ground drag if airborne for better air control
        else
        {
            rigidBody.drag = 0;
        }
    }

    private void LateUpdate()
    {
        DrawGrappleRope();
    }

    private void FixedUpdate()
    {
        // Moves player automatically encorporating Time.deltaTime
        MovePlayer();
    }

    // Function that tracks user input
    private void UserInput()
    {
        // Gets horizontal and vertical input (WASD)
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    // Function to handle movement state changes
    private void StateHandler()
    {
        // Grounded State Handler. All If statements assume you are grounded
        if (grounded)
        {
            if (!doubleJumpReady)
            {
                doubleJumpReady = true;
            }

            if (grappleCount >= grapplesAllowed)
            {
                grappleCount = 0;
            }

            if (Input.GetKeyUp(crouchKey))
            {
                // Resets the player's Y scale to their default scale
                transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
            }

            // Detects Space key press. Allows jump as long as jumpcooldown has expired and is grounded
            if (Input.GetKey(jumpKey) && readyToJump && !isGrappling)
            {
                // jump flag (also prevents crouching since you are in the process of jumping)
                readyToJump = false;

                if (Input.GetKey(crouchKey))
                {
                    // Forces stoping crouch before jumping
                    ForceStopCrouch();
                }

                // Jump function
                Jump();
                // Starts jump cooldown before resetting the jump flag
                Invoke(nameof(ResetJump), jumpCooldown);
            }


            // Sprint Takes Precedence. Sets moveSpeed to sprintSpeed and forces crouching to stop
            if (Input.GetKey(sprintKey))
            {
                // If crouch and sprint are pressed at the same time, sprint takes precidence
                if (Input.GetKey(crouchKey))
                {
                    ForceStopCrouch();
                }

                movementState = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }
            // Crouching takes second precidence. Sets moveSpeed to crouchSpeed and sets Yscale
            else if (Input.GetKey(crouchKey))
            {
                movementState = MovementState.crouching;
                moveSpeed = crouchSpeed;
                if (readyToJump)
                {
                    transform.localScale = new Vector3(transform.localScale.x, crouchYscale, transform.localScale.z);
                }
            }
            // Sets moveSpeed to walkSpeed if grounded and not sprinting or crouching
            else
            {
                movementState = MovementState.walking;
                moveSpeed = walkSpeed;
            }


        }
        // If grappling, movement state stays as grappling
        else if (isGrappling)
        {
            movementState = MovementState.grappling;
        }
        // Airborne State Handler. All if statements assume you are airborne and not grappling
        else
        {

            // moveSpeed set to sprint speed while airborne to allow for faster air movement when sprinting (Takes Precidence over crouching)
            if (Input.GetKey(sprintKey))
            {
                movementState = MovementState.airborne;
                moveSpeed = sprintSpeed;
            }
            // Doesn't allow crouching while mid air (size stays the same) but does allow for lower speed while airborne
            else if (Input.GetKey(crouchKey))
            {
                movementState = MovementState.airborne;
                moveSpeed = crouchSpeed;
            }
            // If neither crouch or sprint are pressed, airborne max speed is set to walk speed
            else
            {
                movementState = MovementState.airborne;
                moveSpeed = walkSpeed;
            }

            if (Input.GetKey(jumpKey) && doubleJumpReady && readyToJump)
            {
                // jump flag (also prevents crouching since you are in the process of jumping)
                doubleJumpReady = false;

                // Jump function
                Jump();
            }


        }

        
        if (Input.GetKeyDown(grappleKey))
        {
            // If airborne and the player hasnt used all their grapples
            if (!grounded && grappleCount < grapplesAllowed)
            {
                // Increment all the grapples theyve used
                grappleCount++;

                // Change the movement state to grappling and start the grapple
                movementState = MovementState.grappling;
                StartGrapple();
            }

            // If grounded their grapple count shouldn't be incremented
            if (grounded)
            {
                // ForceStopCrouch when grappling
                if (Input.GetKey(crouchKey))
                {
                    ForceStopCrouch();
                }

                // Change the movement state to grappling and start the grapple
                movementState = MovementState.grappling;
                StartGrapple();
            }
            
        }
        else if (Input.GetKeyUp(grappleKey) && isGrappling)
        {
            EndGrapple();
            doubleJumpReady = true;
        }
 
    }

    // Function that moves the player
    private void MovePlayer()
    {
        // Creates vector to apply force based on the player's orientation and their directional input
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Checks if you are on a slope and not attempting to jump
        if(OnSlope() && !jumpOnSlope)
        {
            if (!enteredSlope)
            {
                // Get the current speed (preserve movement speed)
                float speed = rigidBody.velocity.magnitude;

                // Get the movement direction along the slope
                Vector3 slopeDirection = GetSlopeMovementDirection();

                // Apply velocity in the direction of the slope
                rigidBody.velocity = slopeDirection * speed;
            }

            // Applies a downward focre to avoid physics mechanics causing you to bounce on ramps if you are
            // moving up the ramp
            if(rigidBody.velocity.y > 0)
            {
                rigidBody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }

            // Get the current horizontal velocity (ignoring Y-axis)
            Vector3 slopeVelocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, rigidBody.velocity.z);
            float currentSpeed = slopeVelocity.magnitude;

            // Check the dot product to determine if the player is trying to change directions
            float dot = Vector3.Dot(slopeVelocity.normalized, GetSlopeMovementDirection());

            // If below moveSpeed OR trying to change directions, allow force to be applied
            if (currentSpeed < moveSpeed || dot < 0)
            {
                rigidBody.AddForce(GetSlopeMovementDirection() * moveSpeed * 20f, ForceMode.Force);
            }
        }

        if(grounded)
        {
            // Get the current horizontal velocity (ignoring Y-axis)
            Vector3 flatVelocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
            float currentSpeed = flatVelocity.magnitude;

            // Check the dot product to determine if the player is trying to change directions
            float dot = Vector3.Dot(flatVelocity.normalized, moveDirection.normalized);

            // If below moveSpeed OR trying to change directions, allow force to be applied
            if (currentSpeed < moveSpeed || dot < 0)
            {
                rigidBody.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
            }
        }
        // Don't allow force if airborne and grappling
        else if (!grounded && !isGrappling)
        {
            // Get the velocity on the x and z axis
            Vector3 flatVelocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
            float currentSpeed = flatVelocity.magnitude;

            // Check the dot product to determine if the player is trying to change directions
            float dot = Vector3.Dot(flatVelocity.normalized, moveDirection.normalized);

            // Allow movement if it's opposing velocity OR if velocity is below moveSpeed
            if (dot < 0 || currentSpeed < moveSpeed)
            {
                // Add force in the given direction multiplied by the air multiplier
                rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            }
            
        }

        // Stops using gravity if you are on a slope to prevent the player from sliding down the ramp when standing still
        rigidBody.useGravity = !OnSlope();
    }

    

    // Function to stop crouch resizing even if crouch is being pressed
    private void ForceStopCrouch()
    {
        // Resets the players scale to their default scale
        transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
    }

    // Function that applies force upward for jump
    private void Jump()
    {
        // Flag to stop downward force when the player is attempting to jump on a ramp
        jumpOnSlope = true;

        // Resets the y velocity so jump velocity is consistant (not added to preexisting vertical velocity positive or negative)
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);

        // Adds force upward multiplied by the jump force
        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    // Function that resets readyToJump for Invoke call on jump cooldown and slope jump flag
    private void ResetJump()
    {
        // Resets Jump Flag
        readyToJump = true;

        // Jumping on slope flag
        jumpOnSlope = false;
    }

    // Function that detects whether the player is on a ground object that is sloped
    private bool OnSlope()
    {
        // Checks all the hit ground objects and collects their colliders in an array
        Collider[] hitColliders = Physics.OverlapBox(groundCheck.position, groundCheckArea, Quaternion.identity, whatIsGround);

        // Loops through the array of colliders
        foreach (Collider hitCollider in hitColliders)
        {
            // Gets the closest point on the collider to the player
            Vector3 closestPoint = hitCollider.ClosestPoint(transform.position);

            /* 
             * Performs a raycast. Explination of each portion:
             * 
             * - "Physics.Raycast()" - Casts a ray and returns a boolean to say it hit an object
             * 
             * - "closestPoint + Vector3.up * 0.1f" : The origin of the ray starts at the closest point on the collider and 
             * adds .01 to the y position of the origin of the array. So it starts slightly above the closest point on the collider
             * 
             * - "Vector3.down" : Shoots the ray downward so the ray hits the object it was positioned above
             * 
             * - "out slopeHit" : Stores the hit object in the slopeHit variable
             * 
             * - "0.2f" : Length of the ray is a floating point number 0.2f, just enough to hit the object below it
             */
            if (Physics.Raycast(closestPoint + Vector3.up * 0.1f, Vector3.down, out slopeHit, 0.2f))
            {
                // Calculates the angle of the normal (the face of the object) it hit
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                // Returns true if the angle is walkable (less than the max slope angle) and is greater than 0 (not flat)
                return angle < maxSlopeAngle && angle != 0;
            }
        }

        // Returns false if no ground was hit, the angle is not walkable, or if the ground is flat (AKA if the player is not on a slope)
        return false;
    }

    // Function that returns a normalized vector of the players movement direction combined with slope angle to add
    // force along the slope rather than directly into it
    private Vector3 GetSlopeMovementDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    // Function to get the slope angle for modifying player velocity when entering a slope
    private float GetSlopeAngle()
    {
        return Vector3.Angle(Vector3.up, slopeHit.normal);
    } 

    // Function to initiate a grapple
    private void StartGrapple()
    {
        // Cast a raycast from the player and store the point it hit in the grappleHit variable
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out grappleHit, maxGrappleDistance))
        {
            // Get the hit object from the grapple hit variable
            grappleHitObject = grappleHit.collider.gameObject;
            // Check to see whether the hit object's layer is in the whatIsGrapplable layer mask
            if ((whatIsGrapplable.value & (1 << grappleHitObject.layer)) != 0)
            {
                // Get the coordinates of the hit location
                grapplePoint = grappleHit.point;

                // Create a new Spring Joint object
                grappleJoint = gameObject.AddComponent<SpringJoint>();
                // Disable autoconfiguring the connected anchor
                grappleJoint.autoConfigureConnectedAnchor = false;
                // Set the connected anchor to the grapple point
                grappleJoint.connectedAnchor = grapplePoint;

                // Get the distance from the grappled point
                distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

                // Set the max distance and minimum distance from the point to be a proportion of the distance from the point
                grappleJoint.maxDistance = distanceFromPoint * .08f;
                grappleJoint.minDistance = distanceFromPoint * .025f;

                // Configure grappleJoint parameters
                grappleJoint.spring = 3.5f;
                grappleJoint.damper = 12f;
                grappleJoint.massScale = 6f;

                // Change the line renderer to allow for 2 points
                lineRenderer.positionCount = 2;
            }
        }
    }

    // Function to end the grapple
    private void EndGrapple()
    {
        // Change the line renderer to have no points to draw a line between
        lineRenderer.positionCount = 0;
        // Destroy the grapple joint created when initating the grapple
        Destroy(grappleJoint);
    }

    // Function to draw the rope when grappling
    private void DrawGrappleRope()
    {
        // If not grappling, don't draw the rope
        if (!isGrappling) return;

        // Set the first position of the line to be the fire point and the second point to be the grapple point
        lineRenderer.SetPosition(0, grappleFirePoint.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    // Function to rotate the arm when grappling
    private void RotateArmOnGrapple()
    {
        if (!isGrappling)
        {
            // If not grappling, set the desired arm rotation to the camera's rotation
            desiredRotation = playerCamera.rotation;
        }
        else
        {
            // If grappling, set the desired rotation to look at the grapple point - the arm position
            desiredRotation = Quaternion.LookRotation(grapplePoint - armGrapple.position);
        }

        // Lerp for a smooth transition from the current rotation to the desired rotation
        armGrapple.rotation = Quaternion.Lerp(armGrapple.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
    }
}
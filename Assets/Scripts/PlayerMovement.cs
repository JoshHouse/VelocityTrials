using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Initialization")]
    private Rigidbody rigidBody;            // Player's rigid body component
    private float horizontalInput;          // Track horizontal key presses (A and D)
    private float verticalInput;            // Track vertical key presses (W and S)
    private Vector3 moveDirection;          // Vector to calculate how to apply force
    private Transform body;                 // Gets the body child object in the start function
    private Renderer bodyRenderer;          // Gets the renderer of the body in the start function
    private float bodyLength;               // Gets the length of the body after rendering from the renderer
    private float bodyWidth;                // Gets the width of the body after rendering from the renderer
    private float bodyHeight;               // Gets the height of the body after rendering from the renderer


    public Transform orientation;           // Empty object attached to player to get their rotation for movement application based on rotation
    public MovementState movementState;     // Current state of the player based on enumeration below
    // Enumeration to track the current state of the player
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        airborne
    }


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


    [Header("Crouching")]
    public float crouchSpeed = 3.5f;        // Movement speed while crouching
    public float crouchYscale = 0.5f;       // Amount the player shrinks when crouching
    private float startYscale;              // Start scale to reset the player after crouching


    [Header("Ground Check")]
    public Transform groundCheck;           // Empty object attached to player to check for the ground beneath them
    public float groundDistance = 0.2f;     // Radius around groundCheck to check if the player is on the ground
    public LayerMask whatIsGround;          // Layer assigned to ground objects to reset jumps and apply ground drag or airMultiplier
    private Vector3 groundCheckArea;        // Vector3 for the area to check for ground under the player (calculated based on size)
    private bool grounded;                  // Grounded flag


    [Header("Slope Handling")]
    public float maxSlopeAngle = 40f;       // Maximum slope the player can walk up
    private RaycastHit slopeHit;            // Detected slope that the player hit
    private bool exitingSlope;              // Flag Variable used to allow jumping on ramps


    // Area for Keybinds so we can apply settings from a settings menu
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;             // Space keybind
    public KeyCode sprintKey = KeyCode.LeftShift;       // Sprint keybind
    public KeyCode crouchKey = KeyCode.LeftControl;     // Crouch keyvind



    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();  // Get Player's Rigidbody component
        rigidBody.freezeRotation = true;        // Freeze RigidBody's rotation

        readyToJump = true;                     // Allow the player to start with jump ready

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
        // Caps max speed so the player doesnt infinitely accelerate
        SpeedControl();
        

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

            if (Input.GetKeyUp(crouchKey))
            {
                // Resets the player's Y scale to their default scale
                transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
            }

            // Detects Space key press. Allows jump as long as jumpcooldown has expired and is grounded
            if (Input.GetKey(jumpKey) && readyToJump)
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
        // Airborne State Handler. All if statements assume you are airborne
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
                movementState = MovementState.walking;
                moveSpeed = walkSpeed;
            }


        }
 
    }

    // Function that prevents infinite acceleration
    private void SpeedControl()
    {
        /*
         * - Checks if you are on a slope (GetSlopeMovementDirection function sets your velocity at the same angle as the 
         *   slope and is called in the move player function based on the OnSlope() function) 
         * - exitSlope is a flag set in the jump function. Move Player adds downward force while on a slope 
         *   because of physics mechanics causing you to bounce while going up ramps. exitingSlope prevents this
         *   downward force to allow the player to jump
         */
        if (OnSlope() && !exitingSlope)
        {
            // Default movement tracks magnitude only on the x and z axis so jumping doesnt lower speed.
            // When on a slope, the vertical magnitude needs to be tracked too. (Except when you jump aka. exitingSlope)
            if (rigidBody.velocity.magnitude > moveSpeed)
            {
                rigidBody.velocity = rigidBody.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            // Gets the velocity (excluding the y axis since you aren't on a slope)
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
    }

    // Function that moves the player
    private void MovePlayer()
    {
        // Creates vector to apply force based on the player's orientation and their directional input
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Checks if you are on a slope and not attempting to jump
        if(OnSlope() && !exitingSlope)
        {
            // GetSlopeMovementDirection returns a Vector3 of the slope angle combined with the movement direction
            // normailzed to 1
            rigidBody.AddForce(GetSlopeMovementDirection() * moveSpeed * 20f, ForceMode.Force);

            // Applies a downward focre to avoid physics mechanics causing you to bounce on ramps if you are
            // moving up the ramp
            if(rigidBody.velocity.y > 0)
            {
                rigidBody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

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
        exitingSlope = true;

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
        exitingSlope = false;
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

}
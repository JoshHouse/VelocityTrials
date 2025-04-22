using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedMovementScript : MonoBehaviour
{
    [Header("Movement Manager Script")]
    public PlayerMovementManager pMm;               // Reference to the Player Movement Manager Script
    public AirborneMovementScript aMs;              // Reference to the airborne movement script for interactions in transitions

    [Header("Movement Mechanic Scripts")]
    public SlidingScript sS;                        // Reference to the sliding script
    public WallClimbingScript wCs;                  // Reference to the climbing script

    [Header("Slope Handling")]
    public float maxSlopeAngle;                     // Maximum slope angle that is walkable
    [HideInInspector] public RaycastHit slopeHit;   // Stores the slope the raycast hit
    [HideInInspector] public bool jumpOnSlope;      // Flag to stop adding downward force when jumping on a slope


    [Header("Crouching")]
    public float crouchYscale;                      // Y Scale of the player when crouching
    [HideInInspector] public float startYscale;     // Default scale of the player

    [Header("Jumping")]
    public float jumpForce;                         // Force applied when jumping
    public float jumpCooldown;                      // Cooldown between jumps
    [HideInInspector] public bool readyToJump;      // Jump flag managed by jump cooldown


    /*
     * 
     * ---------- Initalize values on start ----------
     * 
     */
    private void Start()
    {
        // Ready to jump on start
        readyToJump = true;

        // Store default y scale
        startYscale = transform.localScale.y;
    }

    /*
     * 
     * ---------- Grounded Input Retrieval ----------
     * 
     */

    public void GetGroundedInput()
    {
        // If there is a wall climbable wall, the player inputs the jump key, they are looking into the wall,
        // they aren't already climbing, and they are inputting forward into the wall
        if (wCs.wallInFront &&
            Input.GetKeyDown(pMm.jumpKey) &&
            wCs.wallLookAngle < wCs.maxWallLookAngle &&
            !wCs.isClimbing &&
            pMm.verticalInput > 0)
        {
            // Start climbing
            wCs.StartClimbing();
        }
        // If they are climbing and they stop holding into the wall
        else if (wCs.isClimbing && pMm.verticalInput <= 0)
        {
            // Stop Climbing
            wCs.StopClimbing();
        }
        // If they press the jump key while climbing
        else if (wCs.isClimbing && Input.GetKeyDown(pMm.jumpKey))
        {
            // Stop climbing and wall jump
            wCs.StopClimbing();
            wCs.WallJump();
        }
        // Call jump if flag is ready and jump is pressed
        else if (Input.GetKey(pMm.jumpKey) && readyToJump && !wCs.isClimbing)
        {
            // If sliding, stop sliding when jumping
            if (sS.isSliding)
            {
                sS.StopSlideWithMomentum();
            }

            // If the player is crouching, reset the local scale before jumping
            if (pMm.movementState == PlayerMovementManager.MovementState.crouching)
            {
                transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
            }

            Jump();
        }

        // Starts slide as long as the player is inputting a movement direction, presses the slide key, and isn't already sliding
        else if (Input.GetKeyDown(pMm.slideKey) &&
                (pMm.horizontalInput != 0 || pMm.verticalInput != 0) &&
                !sS.isSliding)
        {
            sS.StartSlide();
        }

        // Stops slide if the user releases the slide key and is sliding
        else if (Input.GetKeyUp(pMm.slideKey) &&
            sS.isSliding)
        {
            sS.StopSlideWithMomentum();
        }

        // Sets y scale to crouch scale if holding crouch button, not jumping, and not sliding
        else if (Input.GetKey(pMm.crouchKey) &&
            readyToJump &&
            !sS.isSliding)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYscale, transform.localScale.z);
        }

        // Resets y scale if user releases the crouch key
        else if (Input.GetKeyUp(pMm.crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
        }
    }

    /*
     * 
     * ---------- Grounded State Handler ----------
     * 
     */

    public void groundedStateHandler()
    {
        // Stops wallrunning if you hit the ground
        if(aMs.wRs.isWallRunning)
        {
            aMs.wRs.StopWallRun();
        }

        // Sets double jump flag to true if you hit the ground
        if (!aMs.doubleJumpReady)
        {
            aMs.doubleJumpReady = true;
        }

        // If they are climbing
        if (wCs.isClimbing)
        {
            // Set their state
            pMm.movementState = PlayerMovementManager.MovementState.wallClimbing;

            // Set Desired Move Speed to wallClimbSideSpeed
            pMm.desiredMoveSpeed = pMm.wallClimbSideSpeed;
        }
        // If they are sliding
        else if (sS.isSliding)
        {
            // Set their state
            pMm.movementState = PlayerMovementManager.MovementState.sliding;


            // If they are sliding down a slope, set their desired slide speed to their max slide speed
            if(OnSlope() &&
                pMm.playerRigidBody.velocity.y < 0.1f)
            {
                // Max slide speed is much higher than other speeds to allow you to build up speed when sliding down a slope
                pMm.desiredMoveSpeed = pMm.slideSpeed;
            }
            // if they are sliding up a slope or not on a slope, set their max speed when sliding to sprint speed
            else
            {
                pMm.desiredMoveSpeed = pMm.sprintSpeed;
            }
        }

        // If they are holding crouch and not sliding
        else if (Input.GetKey(pMm.crouchKey))
        {
            // Set their state
            pMm.movementState = PlayerMovementManager.MovementState.crouching;
            // Set their max speed to crouchSpeed
            pMm.desiredMoveSpeed = pMm.crouchSpeed;
        }

        // If they are holding sprint key
        else if (Input.GetKey(pMm.sprintKey))
        {
            // Set their state
            pMm.movementState = PlayerMovementManager.MovementState.sprinting;
            // Set their max speed to sprint speed
            pMm.desiredMoveSpeed = pMm.sprintSpeed;
        }

        // If not sprinting, sliding, or crouching, they are walking
        else
        {
            // Set their state
            pMm.movementState = PlayerMovementManager.MovementState.walking;
            // Set their max speed to walk speed
            pMm.desiredMoveSpeed = pMm.walkSpeed;
        }

    }

    /*
     * 
     * ---------- Grounded Movement Management ----------
     * 
     */

    public void handleGroundedMovement()
    {
        // If they are climbing
        if(wCs.isClimbing)
        {
            // Call climbing script's movement handler
            wCs.ClimbingMovement();
        }
        // If sliding
        else if (sS.isSliding)
        {
            // Call sliding script's movement handler
            sS.handleSlideMovement();
        }


        // If on a slope and not trying to jump, add slope force
        if (OnSlope() &&
            !jumpOnSlope)
        {
            pMm.playerRigidBody.AddForce(GetSlopeMovementDirection(pMm.moveDirection) * pMm.moveSpeed * 20f, ForceMode.Force);
        }

        // If not on slope or jumping off the slope, add flat force
        else
        {
            pMm.playerRigidBody.AddForce(pMm.moveDirection.normalized * pMm.moveSpeed * 10f, ForceMode.Force);
        }

    }

    /*
     * 
     * ---------- Slope Detection ----------
     * 
     */

    
    public bool OnSlope()
    {
        // Cast a ray down from the center of the player to detect ground
        if (Physics.Raycast(pMm.orientation.position,           // Origin of the ray
                            Vector3.down,                       // Direction of the ray
                            out slopeHit,                       // Where to store the information
                            (pMm.bodyHeight * 0.5f) + 0.3f,     // Ray distance
                            pMm.whatIsGround))                  // Layer mask of what the ray is looking for
        {
            // Calculate the angle of the slope
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            // Return true if the ground hit is an angle and that angle less than the max slope angle
            return angle < maxSlopeAngle && angle != 0;
        }
        // Return false if nothing was hit
        return false;
    }

    public Vector3 GetSlopeMovementDirection(Vector3 direction)
    {
        // returns the direction to add force in based on what the slope angle is
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    /*
     * 
     * ---------- Jump Functionality ----------
     * 
     */

    private void Jump()
    {
        // Ready to jump false and jump on slope true (reset by resetJump function
        readyToJump = false;

        jumpOnSlope = true;

        // Reset y velocity so jump is a consistent height
        pMm.playerRigidBody.velocity = new Vector3(pMm.playerRigidBody.velocity.x, 0f, pMm.playerRigidBody.velocity.z);

        // Add impulse force upward
        pMm.playerRigidBody.AddForce(transform.up * jumpForce,
                                        ForceMode.Impulse);

        // Call the resetJump function to reset flags
        Invoke(nameof(resetJump), jumpCooldown);
    }

    // Resets jump flags
    private void resetJump()
    {
        readyToJump = true;

        jumpOnSlope = false;
    }


}

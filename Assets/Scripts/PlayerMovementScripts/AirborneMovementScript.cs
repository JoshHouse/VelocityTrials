using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneMovementScript : MonoBehaviour
{
    [Header("Movement Manager Script")]
    public PlayerMovementManager pMm;       // Reference to player movement manager
    public GroundedMovementScript gMs;      // Reference to grounded movement script for transition interactions

    [Header("Movement Scripts")]
    public WallRunningScript wRs;           // Reference to the wall running script
    public WallClimbingScript wCs;          // Reference to the climbing script

    public float airMultiplier;             // Air speed multipler

    public bool doubleJumpReady;            // Flag for if the player can double jump


    private void Start()
    {
        doubleJumpReady = true;             // Ready to double jump on start
    }

    /*
     * 
     * ---------- Airborne Input Retrieval ----------
     * 
     */

    public void GetAirborneInput()
    {
        // Shoots raycast to check for wall runnable walls
        wRs.CheckForWall();

        if (wCs.wallInFront &&
            Input.GetKeyDown(pMm.jumpKey) &&
            wCs.wallLookAngle < wCs.maxWallLookAngle &&
            !wCs.isClimbing &&
            pMm.verticalInput > 0)
        {
            wCs.StartClimbing();
        }
        else if (wCs.isClimbing && pMm.verticalInput <= 0)
        {
            wCs.StopClimbing();
        }
        else if (wCs.isClimbing && Input.GetKeyDown(pMm.jumpKey))
        {
            wCs.StopClimbing();
            wCs.WallJump();
        }

        // If wall running and presses the jump key
        else if (wRs.isWallRunning && Input.GetKeyDown(pMm.jumpKey))
        {
            // Stop wall run
            wRs.StopWallRun();
            // Jump away from the wall
            wRs.WallJump();
        }

        // If player pressed jump key, there is a wall in range, and they are inputting forward movement
        else if ((wRs.wallLeft || wRs.wallRight) &&
                Input.GetKeyDown(pMm.jumpKey) &&
                pMm.verticalInput > 0)
        {
            // If not wall running but can wall run
            if (!wRs.isWallRunning && wRs.canWallRun)
            {
                // Start wall run
                wRs.StartWallRun();
            }
        }

        // If there, is no wall on the left or right or the player isnt inputting forward, and is wall running
        else if ((!(wRs.wallLeft || wRs.wallRight) || pMm.verticalInput <= 0)
                && wRs.isWallRunning)
        {
            // Stop wall run
            wRs.StopWallRun();
        }

        // if double jump is ready, the readyToJump flag in the grounded movement script is true, and user inputs jump
        else if (doubleJumpReady && gMs.readyToJump && Input.GetKeyDown(pMm.jumpKey))
        {
            // Jump
            Jump();
        }
    }

    /*
     * 
     * ---------- Airborne State Handler ----------
     * 
     */

    public void airborneStateHandler()
    {
        if (wCs.isClimbing)
        {
            pMm.movementState = PlayerMovementManager.MovementState.wallClimbing;

            // Set desired move speed to wallClimbSideSpeed
            pMm.desiredMoveSpeed = pMm.wallClimbSideSpeed;
        }
        // if wall running flag is active
        else if (wRs.isWallRunning)
        {
            // Set the state
            pMm.movementState = PlayerMovementManager.MovementState.wallRunning;
            // Set the move speed to wall run speed
            pMm.desiredMoveSpeed = pMm.wallRunSpeed;
        }
        // If not wall running but airborne
        else
        {
            // Set the state
            pMm.movementState = PlayerMovementManager.MovementState.airborne;
            // Set the movement speed to air straife speed
            pMm.desiredMoveSpeed = pMm.airStraifeSpeed;
        }

        
        
    }

    /*
     * 
     * ---------- Airborne Movement Management ----------
     * 
     */

    public void handleAirborneMovement()
    {
        if (wCs.isClimbing)
        {
            wCs.ClimbingMovement();
        }
        // If wall running
        if (wRs.isWallRunning)
        {
            // Call wall run movement handler
            wRs.HandleWallRunMovement();
        }
        else
        {
            // Add force in the player's movement direction
            pMm.playerRigidBody.AddForce(pMm.moveDirection.normalized * pMm.moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    /*
     * 
     * ---------- Jump Functionality ----------
     * 
     */

    private void Jump()
    {
        // Ready to jump false and jump on slope true (reset by resetJump function)
        gMs.readyToJump = false;

        gMs.jumpOnSlope = true;

        // Jumping uses your double jump
        doubleJumpReady = false;

        // Reset y velocity so jump is a consistent height
        pMm.playerRigidBody.velocity = new Vector3(pMm.playerRigidBody.velocity.x, 0f, pMm.playerRigidBody.velocity.z);

        // Add force upward
        pMm.playerRigidBody.AddForce(transform.up * gMs.jumpForce, ForceMode.Impulse);

        // Call the resetJump function to reset flags
        Invoke(nameof(resetJump), gMs.jumpCooldown);
    }

    // Resets jump flags
    private void resetJump()
    {
        gMs.readyToJump = true;

        gMs.jumpOnSlope = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneMovementScript : MonoBehaviour
{
    [Header("Initialization")]
    public AnimationManager animManager;

    [Header("Movement Manager Script")]
    public PlayerMovementManager pMm;       // Reference to player movement manager
    public GroundedMovementScript gMs;      // Reference to grounded movement script for transition interactions

    [Header("Movement Scripts")]
    public WallRunningScript wRs;           // Reference to the wall running script

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

        // If wall running and presses the jump key
        if (wRs.isWallRunning && GameManager.instance.jumpPressed)
        {
            // Stop wall run
            wRs.StopWallRun();
            // Jump away from the wall
            wRs.WallJump();
            return;
        }

        // If player pressed jump key and can start a wall run
        if (GameManager.instance.jumpPressed && wRs.CanStartWallRun())
        {
            // Start wall run
            wRs.StartWallRun();
            return;
        }

        // If there, is no wall on the left or right or the player isnt inputting forward, and is wall running
        if ((!(wRs.wallLeft || wRs.wallRight) || pMm.verticalInput <= 0)
                && wRs.isWallRunning)
        {
            // Stop wall run
            wRs.StopWallRun();
            return;
        }

        // if double jump is ready, the readyToJump flag in the grounded movement script is true, and user inputs jump then jump
        if (doubleJumpReady && gMs.readyToJump && GameManager.instance.jumpPressed)
            Jump();
    }

    /*
     * 
     * ---------- Airborne State Handler ----------
     * 
     */

    public void airborneStateHandler()
    {
        // if wall running flag is active
        if (wRs.isWallRunning)
        {
            pMm.gS.predictionVisualizer.SetActive(false);
            // Set the state
            pMm.movementState = PlayerMovementManager.MovementState.wallRunning;
            // Set the move speed to wall run speed
            pMm.desiredMoveSpeed = pMm.wallRunSpeed;
            return;
        }

        // If not wall running but airborne then set the state
        pMm.movementState = PlayerMovementManager.MovementState.airborne;
        // Set the movement speed to air straife speed
        pMm.desiredMoveSpeed = pMm.airStraifeSpeed;

        
        
    }

    /*
     * 
     * ---------- Airborne Movement Management ----------
     * 
     */

    public void handleAirborneMovement()
    {
        // If wall running
        if (wRs.isWallRunning)
        {
            // Call wall run movement handler
            wRs.HandleWallRunMovement();
            return;
        }

        // Add force in the player's movement direction
        pMm.playerRigidBody.AddForce(pMm.moveDirection.normalized * pMm.moveSpeed * 10f * airMultiplier, ForceMode.Force);

        animManager.PlayAnim("Airborne");
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

        animManager.PlayAnim("Jump");

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

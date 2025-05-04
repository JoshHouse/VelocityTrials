using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunningScript : MonoBehaviour
{
    [Header("Initialization")]
    public AnimationManager animManager;

    [Header("Movement Manager Script")]
    public PlayerMovementManager pMm;               // Reference to the player movement manager
    public AirborneMovementScript aMs;              // Reference to the airborne movement script

    [Header("WallRunning")]
    public LayerMask whatIsWall;                    // Layer mask for wall runnable walls
    public float wallRunForce;                      // Force applied when wall running
    public float wallJumpUpForce;                   // How much force the player gets upward when wall jumping
    public float wallJumpSideForce;                 // How much side force the player gets when wall jumping
    public float wallRunCooldown;                   // Cooldown between wall runs
    public float maxWallRunTime;                    // How long the player can wall run for
    private float wallRunTimer;                     // Timer for how long the player has been wall running
    [HideInInspector] public bool isWallRunning;    // Flag for if player is wall running
    [HideInInspector] public bool canWallRun;       // Flag for if the player can wall run managed by wall run cooldown

    [Header("Detection")]
    public float wallCheckDistance;                 // Distance for how far away the player can be from the wall to start a wall run
    private RaycastHit leftWallHit;                 // Raycast hit to store a wall hit by left raycast
    private RaycastHit rightWallHit;                // Raycast hit to store a wall hit by right raycast
    [HideInInspector] public bool wallLeft;         // Flag for if a wall was detected on the left
    [HideInInspector] public bool wallRight;        // Flag for if a wall was detected on the right

    [Header("Cam Rotation")]
    public float rotationDegrees;                   // How much rotation the camera should get when on a wall
    public float rotationSmoothing;                 // Smooths the camera rotation


    void Start()
    {
        canWallRun = true;      // sets can wall run flag to true on start
    }

    /*
     *
     * ---------- Detection ----------
     *
     */

    public void CheckForWall()
    {
        // Shoots a raycast for right wall detection
        wallRight = Physics.Raycast(pMm.orientation.position,                   // Ray origin
                                    pMm.orientation.right,                      // Ray direction (right of orientation)
                                    out rightWallHit,                           // Where to store the object hit
                                    (pMm.bodyWidth / 2) + wallCheckDistance,    // Ray distance
                                    whatIsWall);                                // Layer mask of what to look for
        // Shoots a raycast for left wall detection
        wallLeft = Physics.Raycast(pMm.orientation.position,                    // Ray Origin
                                -pMm.orientation.right,                         // Ray direction (left of orientation)
                                out leftWallHit,                                // Where to store the object hit
                                (pMm.bodyWidth / 2) + wallCheckDistance,        // Ray distance
                                whatIsWall);                                    // Layer mask of what to look for
    }

    /*
     *
     * ---------- Wall run handling ----------
     *
     */

    public void StartWallRun()
    {
        // Don't use gravity when wall running
        pMm.playerRigidBody.useGravity = false;
        // Reset the y velocity so the player only moves horizontally
        pMm.playerRigidBody.velocity = new Vector3(pMm.playerRigidBody.velocity.x, 0f, pMm.playerRigidBody.velocity.z);

        if (wallRight)
        {
            animManager.PlayAnim("WallRunRight");
        }
        else
        {
            animManager.PlayAnim("WallRunLeft");
        }

        // Set wall running flag
        isWallRunning = true;

        // Reset the wall run timer
        wallRunTimer = maxWallRunTime;
        
    }

    public void HandleWallRunMovement()
    {
        // Decrease the timer if the player still has time to wall run
        if (wallRunTimer > 0)
        {
            wallRunTimer -= Time.deltaTime;
        }
        else
        {
            // Stop wall run
            StopWallRun();
            /*
             
             *NOTE* wall run cooldown only applies when the player runs out of time wall running. 
             It does not apply when the player jumps off the wall
             
             */

            // Set canWall run to false
            canWallRun = false;
            // Call reset wall run function to reset the can wall run flag after the cooldown
            Invoke(nameof(ResetWallRun), wallRunCooldown);
        }

        // Get the normal of the wall the player is wall running on
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        // Get the forward direction of the wall (forward direction along the wall)
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // Check if the direction the player is trying to run along the wall is forward or backward
        if((pMm.orientation.forward - wallForward).magnitude > (pMm.orientation.forward - -wallForward).magnitude)
        {
            // Invert the forward direction if the player is trying to run in the backward direction of the wall
            wallForward = -wallForward;
        }

        // add force to the player along the wall forward direction
        pMm.playerRigidBody.AddForce(wallForward * wallRunForce, ForceMode.Force);

        pMm.playerModel.rotation = Quaternion.LookRotation(-wallForward);

        // add force into the wall to make the player stick to the wall
        pMm.playerRigidBody.AddForce(-wallNormal * 100f, ForceMode.Force);
    }

    public void StopWallRun()
    {
        // Use gravity again when the player stops wall running
        pMm.playerRigidBody.useGravity = true;

        // Reset wall running flag
        isWallRunning = false;

    }

    /*
     *
     * ---------- Wall Jump Functionality and Cooldown Reset ----------
     *
     */

    public void WallJump()
    {
        // Gets the wall normal of the wall the player is on
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        if (wallRight)
        {
            animManager.PlayAnim("WallJumpRight");
        }
        else
        {
            animManager.PlayAnim("WallJumpLeft");
        }

        // Applies wall jump up force and wall jump side force
        Vector3 totalJumpForce = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // Resets the player's y velocity for consistent jump height
        pMm.playerRigidBody.velocity = new Vector3(pMm.playerRigidBody.velocity.x, 0f, pMm.playerRigidBody.velocity.z);

        // Applies the jump force calculated with wall jump up and wall jump side force applied
        pMm.playerRigidBody.AddForce(totalJumpForce, ForceMode.Impulse);
    }

    // Reset wall run to manage wall run cooldown
    public void ResetWallRun()
    {
        canWallRun = true;
    }

    public bool CanStartWallRun()
    {
        return pMm.verticalInput > 0 && !isWallRunning && canWallRun && (wallLeft || wallRight);
    }
}

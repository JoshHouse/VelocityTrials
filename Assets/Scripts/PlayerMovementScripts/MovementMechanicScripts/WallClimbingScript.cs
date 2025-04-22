using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallClimbingScript : MonoBehaviour
{
    [Header("Movement Manager Script")]
    public PlayerMovementManager pMm;               // Reference to the player movement manager
    public GroundedMovementScript gMs;              // Reference to the grounded movement script
    public AirborneMovementScript aMs;              // Reference to the airborne movement script

    [Header("Climbing")]
    public float maxClimbTime;                      // Maximum amount of time the player can climb
    public float wallClimbCooldown;                 // Cooldown on climbing if the player lets the timer run out
    public LayerMask whatIsWall;                    // Layer mask of what is wall climbable
    public float wallJumpUpForce;                   // How much force the player gets upward when wall jumping
    public float wallJumpBackForce;                 // How much back force the player gets when wall jumping
    private float climbTimer;                       // Timer for how long the player has been climbing for
    [HideInInspector] public bool isClimbing;       // Flag for if the player is climbing
    [HideInInspector] public bool canWallClimb;     // Flag for if the player can climb a wall

    [Header("Detection")]
    public float detectionLength;                   // Distance from the wall the player can be from the wall to intitiate wall climb
    public float sphereCastRadius;                  // Radius of the cylinder that the raycast will cast
    public float maxWallLookAngle;                  // Maximum angle into the wall the player can look to initiate a wall climb
    [HideInInspector] public float wallLookAngle;   // Angle the player is looking into the wall

    private RaycastHit frontWallHit;                // RaycastHit to hold the wall information of the wall hit by the spherecast
    [HideInInspector] public bool wallInFront;      // Boolean for if there is a wall in front

    // Start is called before the first frame update
    void Start()
    {
        canWallClimb = true;        // Set can climb wall to true by default
    }

    /*
     * 
     * ---------- Wall Detection ----------
     * 
     */

    public void WallCheck()
    {
        // SphereCast is like a Raycast except it casts a cylinder
        wallInFront = Physics.SphereCast(pMm.orientation.position,      // Origin of the spherecast
                                        sphereCastRadius,               // Radius of the sphere
                                        pMm.orientation.forward,        // Direction of the cast
                                        out frontWallHit,               // Where to store the hit information
                                        detectionLength,                // Length of the cast
                                        whatIsWall);                    // Layer mask of what to look for

        // Calculate the angle you are looking at the wall from (prevents wall running and wall climbing at the same time)
        wallLookAngle = Vector3.Angle(pMm.orientation.forward, -frontWallHit.normal);
    }

    /*
     * 
     * ---------- Wall Climb Functionality Handling ----------
     * 
     */

    public void StartClimbing()
    {
        // Set the is climbing variable to true
        isClimbing = true;

        // Reset the climb timer to the max climb time
        climbTimer = maxClimbTime;

        // Reset the double jump flag in the airborne movement script
        aMs.doubleJumpReady = true;
    }

    public void ClimbingMovement()
    {
        // If the climb timer isnt 0 yet, and the player is still looking at an acceptable angle
        if (climbTimer > 0 && wallLookAngle < maxWallLookAngle)
        {
            // Manually set the y velocity of the player (more simple than adding force since dynamic upward movement isnt necessary for climbing)
            pMm.playerRigidBody.velocity = new Vector3(pMm.playerRigidBody.velocity.x, pMm.wallClimbUpSpeed, pMm.playerRigidBody.velocity.z);

            // Decrease the timer
            climbTimer -= Time.deltaTime;
        }
        else
        {
            // If the player runs out of time or looks away from the wall, stop climbing
            StopClimbing();
        }
    }

    public void StopClimbing()
    {
        // deactivate the is climbing flag
        isClimbing = false;

        // deactivate the canWallClimb flag
        canWallClimb = false;

        // Call the resetWallClimb function to reset the canWallClimb flag after the wallClimbCooldown
        Invoke(nameof(ResetWallClimb), wallClimbCooldown);
    }

    /*
     * 
     * ---------- Wall Jump Functionality and Cooldown Reset ----------
     * 
     */

    public void WallJump()
    {
        // Gets the wall normal of the wall the player is on
        Vector3 wallNormal = frontWallHit.normal;

        // Applies wall jump up force and wall jump side force
        Vector3 totalJumpForce = transform.up * wallJumpUpForce + wallNormal * wallJumpBackForce;

        // Resets the player's y velocity for consistent jump height
        pMm.playerRigidBody.velocity = new Vector3(pMm.playerRigidBody.velocity.x, 0f, pMm.playerRigidBody.velocity.z);

        // Applies the jump force calculated with wall jump up and wall jump side force applied
        pMm.playerRigidBody.AddForce(totalJumpForce, ForceMode.Impulse);
    }

    // Reset wall run to manage wall run cooldown
    public void ResetWallClimb()
    {
        canWallClimb = true;
    }
}

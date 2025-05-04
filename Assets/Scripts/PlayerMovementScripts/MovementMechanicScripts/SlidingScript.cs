using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingScript : MonoBehaviour
{
    [Header("Initialization")]
    public AnimationManager animManager;

    [Header("Movement Manager Scripts")]
    public PlayerMovementManager pMm;           // Reference to the player movement manager
    public GroundedMovementScript gMs;          // Reference to grounded movement script since sliding can only be performed grounded

    [Header("Slide Parameters")]
    public float maxSlideTime;                  // Max time the player can slide
    public float slideForce;                    // Force applied when sliding
    public float permSlideMinVelocity;          // If moving fast enough, allow the slide to continue
    private float slideTimer;                   // Timer to track how long the player has been sliding

    [HideInInspector] public bool isSliding;    // Flag to tell grounded script when you are sliding



    private void Start()
    {
        isSliding = false;  // Not sliding on start
    }

    /*
     * 
     * ---------- Slide Functionality Handling ----------
     * 
     */

    public void StartSlide()
    {
        // Set sliding flag to true
        isSliding = true;

        animManager.PlayAnim("Slide");

        // Shrink y scale
        transform.localScale = new Vector3(transform.localScale.x, gMs.crouchYscale, transform.localScale.z);

        // Reset the slide timer
        slideTimer = maxSlideTime;
    }

    public void handleSlideMovement()
    {
        // If they are not on a slope or moving up a slope
        if(!gMs.OnSlope() ||
            pMm.playerRigidBody.velocity.y > -0.1f)
        {
            // Add force to the rigid body
            pMm.playerRigidBody.AddForce(pMm.moveDirection.normalized * slideForce,
                                            ForceMode.Force);

            // If their velocity is below the permSlideMinVelocity
            if (pMm.moveSpeed < permSlideMinVelocity)
            {
                // Decrease the slide timer
                slideTimer -= Time.deltaTime;
            }

        }
        else
        {
            // Add force in the slope direction
            pMm.playerRigidBody.AddForce(gMs.GetSlopeMovementDirection(pMm.moveDirection) * slideForce,
                                            ForceMode.Force);
        }


        // If their timer is up, stop the slide
        if(slideTimer <= 0)
        {
            StopSlideWithoutMomentum(); 
        }
    }

    /*
     * 
     * ---------- Stop Functionality ----------
     *  *Note* Can stop with or without momentum for different stop sliding conditions
     */

    public void StopSlideWithoutMomentum()
    {
        // Set Sliding Flag to false
        isSliding = false;

        pMm.moveSpeed = pMm.desiredMoveSpeed;

        // Reset y scale back to default
        transform.localScale = new Vector3(transform.localScale.x, gMs.startYscale, transform.localScale.z);
    }

    public void StopSlideWithMomentum()
    {
        // Set Sliding Flag to false
        isSliding = false;

        // Reset y scale back to default
        transform.localScale = new Vector3(transform.localScale.x, gMs.startYscale, transform.localScale.z);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedMovementScript : MonoBehaviour
{
    [Header("Movement Manager Script")]
    public PlayerMovementManager playerMovementManager;

    [Header("Movement Mechanic Scripts")]
    public SlidingScript slidingScript;
    public MantlingScript mantlingScript;
    public WallClimbingScript wallClimbingScript;
    public GrapplingScript grapplingScript;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void handleGroundedMovement()
    {
        if (Input.GetKey(playerMovementManager.grappleKey))
        {
            grapplingScript.handleGrapple();
        }

        else if (playerMovementManager.movementState == PlayerMovementManager.MovementState.grappling &&
                    Input.GetKeyUp(playerMovementManager.grappleKey))
        {
            grapplingScript.stopGrapple();
        }

        else if  (wallClimbingScript.canWallClimb() &&
                    Input.GetKeyDown(playerMovementManager.jumpKey))
        {
            wallClimbingScript.handleWallClimb();
        }

        else if (playerMovementManager.movementState == PlayerMovementManager.MovementState.wallClimbing &&
                    Input.GetKeyDown(playerMovementManager.jumpKey))
        {
            wallClimbingScript.wallJump();
        }

        else if (mantlingScript.canMantle() &&
            Input.GetKeyDown(playerMovementManager.jumpKey))
        {
            mantlingScript.handleMantle();
        }

        else if (canJump() && Input.GetKey(playerMovementManager.jumpKey))
        {
            handleJump();
        }

        else if (slidingScript.canSlide() &&
                    Input.GetKeyDown(playerMovementManager.crouchKey))
        {
            slidingScript.handleSlide();
        }

        else if (playerMovementManager.movementState == PlayerMovementManager.MovementState.sliding &&
                    Input.GetKeyDown(playerMovementManager.crouchKey))
        {
            slidingScript.stopSlide();
        }

        else if (Input.GetKey(playerMovementManager.sprintKey))
        {
            handleSprint();
        }

        else if (Input.GetKey(playerMovementManager.crouchKey))
        {
            handleCrouch();
        }

        else if (playerMovementManager.horizontalInput != 0 && playerMovementManager.verticalInput != 0)
        {
            handleWalk();
        }

        else
        {
            playerMovementManager.movementState = PlayerMovementManager.MovementState.standing;
        }
    }

    public bool canJump()
    {
        return true;
    }

    public void handleJump()
    {

    }

    public void handleSprint()
    {

    }

    public void handleCrouch()
    {

    }

    public void handleWalk()
    {

    }
}

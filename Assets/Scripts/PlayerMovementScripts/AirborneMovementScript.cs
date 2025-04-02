using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneMovementScript : MonoBehaviour
{
    [Header("Movement Manager Script")]
    public PlayerMovementManager playerMovementManager;

    [Header("Movement Mechanic Scripts")]
    public MantlingScript mantlingScript;
    public WallRunningScript wallRunningScript;
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

    public void handleAirborneMovement()
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

        else if (wallClimbingScript.canWallClimb() &&
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

        else if (wallRunningScript.canWallRun() &&
            Input.GetKeyDown(playerMovementManager.jumpKey))
        {
            wallRunningScript.handleWallRun();
        }

        else if (playerMovementManager.movementState == PlayerMovementManager.MovementState.wallRunning &&
                    Input.GetKeyDown(playerMovementManager.jumpKey))
        {
            wallRunningScript.wallJump();
        }

        else if (canDoubleJump() && Input.GetKey(playerMovementManager.jumpKey))
        {
            handleDoubleJump();
        }

        else if (playerMovementManager.horizontalInput != 0 && playerMovementManager.verticalInput != 0)
        {
            handleAirStraife();
        }

        else
        {
            playerMovementManager.movementState = PlayerMovementManager.MovementState.airborne;
        }

    }

    public bool canDoubleJump()
    {
        return true;
    }

    public void handleDoubleJump()
    {

    }

    public void handleAirStraife()
    {

    }
}

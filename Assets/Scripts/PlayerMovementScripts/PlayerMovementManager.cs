using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    [Header("Initialization")]
    public Transform orientation;           // Empty object attached to player to get their rotation for movement application based on rotation
    public MovementState movementState;     // Current state of the player based on enumeration below
    public enum MovementState               // Enumeration to track the current state of the player
    {
        standing,
        walking,
        sprinting,
        crouching,
        sliding,
        airborne,
        mantling,
        wallRunning,
        wallClimbing,
        grappling
    }
    private Rigidbody rigidBody;            // Player's rigid body component
    private Transform body;                 // Gets the body child object in the start function
    private Renderer bodyRenderer;          // Gets the renderer of the body in the start function
    private float bodyLength;               // Gets the length of the body after rendering from the renderer
    private float bodyWidth;                // Gets the width of the body after rendering from the renderer
    private float bodyHeight;               // Gets the height of the body after rendering from the renderer

    // Area for Keybinds so we can apply settings from a settings menu
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;             // Space keybind
    public KeyCode sprintKey = KeyCode.LeftShift;       // Sprint keybind
    public KeyCode crouchKey = KeyCode.LeftControl;     // Crouch keybind
    public KeyCode grappleKey = KeyCode.Mouse0;         // Grapple keybind

    [Header("Movement Scripts")]
    public GroundedMovementScript groundedMovementScript;
    public AirborneMovementScript airborneMovementScript;
    public SlidingScript slidingScript;
    public MantlingScript mantlingScript;
    public WallRunningScript wallRunningScript;
    public WallClimbingScript wallClimbingScript;
    public GrapplingScript grapplingScript;

    [Header("Ground Check")]
    public Transform groundCheck;           // Empty object attached to player to check for the ground beneath them
    public float groundDistance = 0.2f;     // Radius around groundCheck to check if the player is on the ground
    public LayerMask whatIsGround;          // Layer assigned to ground objects to reset jumps and apply ground drag or airMultiplier
    private Vector3 groundCheckArea;        // Vector3 for the area to check for ground under the player (calculated based on size)
    private bool grounded;                  // Grounded flag



    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();                  // Get Player's Rigidbody component
        rigidBody.freezeRotation = true;                        // Freeze RigidBody's rotation

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

    // Update is called once per frame
    void Update()
    {
        // Checks in a box around the groundCheck empty object for objects with whatIsGround layer title
        // to detect if the player is grounded
        grounded = Physics.CheckBox(groundCheck.position, groundCheckArea, Quaternion.identity, whatIsGround);

        StateHandler();
    }

    private void FixedUpdate()
    {
        // Moves player automatically encorporating Time.deltaTime
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (grounded)
        {
            groundedMovementScript.handleGroundedMovement();
        }
        else
        {
            airborneMovementScript.handleAirborneMovement();
        }
    }

    private void StateHandler()
    {

    }
}

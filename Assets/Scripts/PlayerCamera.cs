using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    // X and Y look sensitivity
    public float sensitivityX = 400f;
    public float sensitivityY = 400f;

    // Empty game object attached to player object for tracking orientation of the player
    public Transform orientation;

    // Rotation variables to track the rotation the camera should be in
    private float rotationX;
    private float rotationY;
    private float rotationZ;

    // Reference to player movement script
    public PlayerMovement playerMoveScript;

    // Rotation for player to lean on z-axis when wall-running
    public float wallRunLeanAngle;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        Cursor.visible = false;                     // Makes the cursor invisible
    }


    void Update()
    {
        // Gets mouse X and Y input multiplied by look sensitivity
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY;

        // Im not too sure why this works but this is how rotation is calculated using unity
        rotationY += mouseX;
        rotationX -= mouseY;

        // Locks the x rotation so you cant look up greater than 90 degrees or down less than -90 degrees
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Rotates camera along Z-axis to lean right or left depending on WallRunningState()
        rotationZ = playerMoveScript.WallRunningCamRotation() * wallRunLeanAngle;

        // Rotates the camera based on the calculated rotation
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);

        // Rotates the orientation attached to the player object (only on the y axis, not vertically)
        orientation.rotation = Quaternion.Euler(0, rotationY, 0);
    }
}
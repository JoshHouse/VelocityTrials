using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Initialization")]
    public Transform orientation;       // Reference to the orientation of the player
    public Transform cameraTracker;

    [Header("Movement Scripts")]
    public WallRunningScript wRs;       // Reference to wall running script for wall running lean

    [Header("Sensitivity")]
    private float sensitivity;   // Camera sensitivity

    private float rotationX;            // Vertical rotation of the camera
    private float rotationY;            // Horizontal rotation of the camera
    private float rotationZ;            // Lean rotation of the camera
    private float targetRotationZ;      // Target lean rotation of the camera for lean smoothing

    void Start()
    {
        // On start, set both the sensitivity variable to the sensitivity value stored in GameManager
        sensitivity = GameManager.instance.pSensitivity;

        Cursor.lockState = CursorLockMode.Locked;   // Locks the cursor to the center of the screen
        Cursor.visible = false;                     // Makes the cursor invisible
    }


    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity;

        // Rotate camera
        rotationY += mouseX;
        rotationX -= mouseY;
        // Clamp vertical rotation to -90 (directly down) and 90 (directly up)
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Determine target Z rotation for wall running lean
        if (wRs.isWallRunning)
        {
            // Invert rotation if wall is on the left
            targetRotationZ = (wRs.wallRight ? 1f : -1f) * wRs.rotationDegrees;
        }
        else
        {
            // If not wall running, target rotation should be 0
            targetRotationZ = 0f;
        }

        // Smoothly interpolate between current rotationZ and targetRotationZ
        rotationZ = Mathf.Lerp(rotationZ, targetRotationZ, Time.deltaTime * wRs.rotationSmoothing);

        // Apply rotation to the camera
        cameraTracker.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
        // Only rotate y axis rotation to the orientation
        orientation.rotation = Quaternion.Euler(0f, rotationY, 0f);

    }
}
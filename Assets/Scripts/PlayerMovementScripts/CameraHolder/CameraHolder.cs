using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    public PlayerMovementManager pMm;

    // cameraPosition empty game object attached to the player
    public Transform FirstPersonCameraPosition;
    public Transform ThirdPersonCameraPosition;

    public float cameraCollisionBuffer = 0.2f; // How far from the hit point the camera should stop


    void Update()
    {
        if (pMm.firstPerson)
        {
            transform.position = FirstPersonCameraPosition.position;
            transform.rotation = FirstPersonCameraPosition.rotation;
            return;
        }

        Vector3 origin = FirstPersonCameraPosition.position;
        Vector3 target = ThirdPersonCameraPosition.position;
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            // If hit, place the camera just in front of the obstacle
            transform.position = hit.point - direction * cameraCollisionBuffer;
        }
        else
        {
            // No obstruction, go to full third-person position
            transform.position = target;
        }

        // Moves the camera to the location of the cameraPosition object attached to the player
        transform.rotation = ThirdPersonCameraPosition.rotation;
    }
}
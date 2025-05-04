using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    public PlayerMovementManager pMm;

    // cameraPosition empty game object attached to the player
    public Transform FirstPersonCameraPosition;
    public Transform ThirdPersonCameraPosition;

    
    void Update()
    {
        if (pMm.firstPerson)
        {
            transform.position = FirstPersonCameraPosition.position;
            transform.rotation = FirstPersonCameraPosition.rotation;
            return;
        }

        // Moves the camera to the location of the cameraPosition object attached to the player
        transform.position = ThirdPersonCameraPosition.position;
        transform.rotation = ThirdPersonCameraPosition.rotation;
    }
}
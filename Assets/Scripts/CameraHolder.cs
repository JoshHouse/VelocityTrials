using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    // cameraPosition empty game object attached to the player
    public Transform cameraPosition;

    
    void Update()
    {
        // Moves the camera to the location of the cameraPosition object attached to the player
        transform.position = cameraPosition.position;
    }
}
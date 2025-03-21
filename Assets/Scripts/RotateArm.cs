using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateArm : MonoBehaviour
{
    public GrappleMechanic grapplingScript;
    public float rotationSpeed = 5f;

    private Quaternion desiredRotation;
    

    private void Update()
    {
        if (!grapplingScript.IsGrappling())
        {
            desiredRotation = transform.parent.rotation;
        }
        else
        {
            desiredRotation = Quaternion.LookRotation(grapplingScript.getGrapplingPoint() - transform.position);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
    }
}

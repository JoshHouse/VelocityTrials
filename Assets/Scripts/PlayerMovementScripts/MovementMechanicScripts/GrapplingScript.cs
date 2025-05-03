using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingScript : MonoBehaviour
{
    [Header("Initialization")]
    public AnimationManager animManager;

    [Header("Movement Manager Script")]
    public PlayerMovementManager pMm;

    [Header("Grapple Parameters")]
    public LayerMask whatIsGrapplable;      // Layer mask for what is grapplable
    public Transform grappleFirePoint;      // Reference to the transform component of the fire point on the grapple arm
    public Transform playerCamera;          // Reference to the playerCamera's transform component
    public Transform armGrapple;            // Reference to the grapple arm's transform component
    public GameObject predictionVisualizer; // Visualizes prediction grapple point
    public int grapplesAllowed;             // Amount of allowed grapples before touching the ground again
    public int grappleCount;                // Count of current grapples used while in the air
    public float maxGrappleDistance;        // Max distance you can grapple
    public float predictionRadius;

    private LineRenderer lineRenderer;      // Reference to the line renderer of the grapple arm
    private Vector3 grapplePoint;           // Vector3 of the point you are grappling to
    private RaycastHit grappleHit;          // Variable to store the raycast response
    private SpringJoint grappleJoint;       // SpringJoint added to the player for grapple mechanics
    private float distanceFromPoint;        // Distance from fire point to the grapple location
    private Quaternion desiredRotation;     // Variable to store what the desired rotation of the arm should be


    [Header("Pull Grapple Parameters")]
    public float movementDelay;
    public float heightGain;
    public bool isPullGrappling;

    [Header("Swing Grapple Parameters")]
    public bool isSwingGrappling;                // Flag for whether the player is grappling
    public float rotationSpeed;             // Rotation speed of the arm when grappling



    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = armGrapple.GetComponent<LineRenderer>(); // Get the line renderer for rope rendering when grappling
        isSwingGrappling = false;                                    // Initialize isGrappling to false when the game starts
        isPullGrappling = false;
        grappleCount = 0;                                       // Initialize the amount of grapples used in the air to 0
    }

    /*
     * 
     * --------------- Swing Grapple Functionality ---------------
     * 
     */

    public bool checkForGrappleLocation()
    {
        if (isPullGrappling || isSwingGrappling) return false;

        RaycastHit sphereCastHit;
        Physics.SphereCast(playerCamera.position,   // Origin of the ray
                            predictionRadius,       // Radius of the spherecast
                            playerCamera.forward,   // Direction of the ray
                            out sphereCastHit,      // Where to store the information
                            maxGrappleDistance,     // Length of the ray
                            whatIsGrapplable);      // Layermask of what objects to look for

        RaycastHit rayHit;
        Physics.Raycast(playerCamera.position,      // Origin of the ray
                            playerCamera.forward,   // Direction of the ray
                            out rayHit,             // Where to store the information
                            maxGrappleDistance,     // Length of the ray
                            whatIsGrapplable);      // Layermask of what objects to look for

        if (rayHit.point != Vector3.zero)
        {
            grappleHit = rayHit;
            predictionVisualizer.transform.position = grappleHit.point;
            predictionVisualizer.SetActive(true);
            return true;
        }

        if (sphereCastHit.point != Vector3.zero)
        {
            grappleHit = sphereCastHit;
            predictionVisualizer.transform.position = grappleHit.point;
            predictionVisualizer.SetActive(true);
            return true;
        }

        predictionVisualizer.transform.position = pMm.orientation.position;
        predictionVisualizer.SetActive(false);
        return false;

    }


    // Function to initiate a grapple
    public void StartSwingGrapple()
    {
        predictionVisualizer.transform.position = pMm.orientation.position;
        predictionVisualizer.SetActive(false);

        animManager.PlayAnim("ShootGrapple");

        // Set grappling flag to true
        isSwingGrappling = true;

        grappleCount++;

        // Get the coordinates of the hit location
        grapplePoint = grappleHit.point;

        // Create a new Spring Joint object
        grappleJoint = gameObject.AddComponent<SpringJoint>();
        // Disable autoconfiguring the connected anchor
        grappleJoint.autoConfigureConnectedAnchor = false;
        // Set the connected anchor to the grapple point
        grappleJoint.connectedAnchor = grapplePoint;

        // Get the distance from the grappled point
        distanceFromPoint = Vector3.Distance(grappleFirePoint.position, grapplePoint);

        // Set the max distance and minimum distance from the point to be a proportion of the distance from the point
        grappleJoint.maxDistance = distanceFromPoint * .08f;
        grappleJoint.minDistance = distanceFromPoint * .025f;

        // Configure grappleJoint parameters
        grappleJoint.spring = 3.5f;
        grappleJoint.damper = 12f;
        grappleJoint.massScale = 6f;

        // Change the line renderer to allow for 2 points
        lineRenderer.positionCount = 2;
    }

    // Function to end the grapple
    public void EndSwingGrapple()
    {
        // Set grappling flag to false
        isSwingGrappling = false;

        // Change the line renderer to have no points to draw a line between
        lineRenderer.positionCount = 0;

        // Destroy the grapple joint created when initating the grapple
        Destroy(grappleJoint);
    }

    /*
     * 
     * ---------- Pull Grapple Functionality ----------
     * 
     */

    public void StartPullGrapple()
    {
        predictionVisualizer.transform.position = pMm.orientation.position;
        predictionVisualizer.SetActive(false);

        pMm.playerRigidBody.drag = 0f;

        animManager.PlayAnim("ShootGrapple");

        isPullGrappling = true;

        grappleCount++;

        // Get the coordinates of the hit location
        grapplePoint = grappleHit.point;

        float highestPointOnArc = Mathf.Max(grapplePoint.y, pMm.orientation.position.y) + heightGain - pMm.orientation.position.y;

        pMm.playerRigidBody.velocity = Vector3.zero;

        JumpToPosition(grapplePoint, highestPointOnArc);

        // Change the line renderer to allow for 2 points
        lineRenderer.positionCount = 2;

        StartCoroutine(WaitForJumpApex());
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        pMm.playerRigidBody.velocity = CalculatePullVelocity(pMm.orientation.position, targetPosition, trajectoryHeight);
    }

    public void StopPullGrapple()
    {
        pMm.playerRigidBody.drag = pMm.groundedDrag;

        // Set grappling flag to false
        isPullGrappling = false;

        // Change the line renderer to have no points to draw a line between
        lineRenderer.positionCount = 0;
    }

    public Vector3 CalculatePullVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);

        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
                    + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    private IEnumerator WaitForJumpApex()
    {
        // Wait until vertical velocity is close to zero (i.e., peak of arc)
        while (Mathf.Abs(pMm.playerRigidBody.velocity.y) > 0.1f)
        {
            yield return null; // Wait for next frame
        }

        StopPullGrapple();
    }


    /*
     * 
     * ---------- Grapple VFX ----------
     * 
     */

    // Function to draw the rope when grappling
    public void DrawGrappleRope()
    {
        // If not grappling, don't draw the rope
        if (!isSwingGrappling && !isPullGrappling) return;

        // Set the first position of the line to be the fire point and the second point to be the grapple point
        lineRenderer.SetPosition(0, grappleFirePoint.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    // Function to rotate the arm when grappling
    public void RotateArmOnGrapple()
    {
        if (!isSwingGrappling)
        {
            // If not grappling, set the desired arm rotation to the camera's rotation
            desiredRotation = playerCamera.rotation;
        }
        else
        {
            // If grappling, set the desired rotation to look at the grapple point - the arm position
            desiredRotation = Quaternion.LookRotation(grapplePoint - armGrapple.position);
        }

        // Lerp for a smooth transition from the current rotation to the desired rotation
        armGrapple.rotation = Quaternion.Lerp(armGrapple.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
    }

    /*
     * 
     * ---------- Helper Functions ----------
     * 
     */

    public bool CanGrapple()
    {
        // return if the player has a valid grapple location and they have enough grapples
        return grappleCount < grapplesAllowed && checkForGrappleLocation() ; 
    }
}

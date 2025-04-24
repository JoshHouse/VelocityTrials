using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MantlingScript : MonoBehaviour
{
    [Header("Movement Manager Script")]
    public PlayerMovementManager pMm;       // Reference to the player movement manager

    [Header("Mantling")]
    public Transform playerObj;
    public float maxMantleDistance;
    public float maxMantleObjectHeight;
    public LayerMask whatIsMantlable;
    public float mantleLength;
    public float mantleYScale;

    [HideInInspector] public bool isMantling;
    [HideInInspector] public float startYscale;
    [HideInInspector] public float playerSemiCircleRadius;
    [HideInInspector] public float forwardRayDistance;
    [HideInInspector] public float topRayDistance;
    [HideInInspector] public RaycastHit forwardMantleHit;
    [HideInInspector] public RaycastHit topMantleHit;
    [HideInInspector] public Vector3 forwardRayOrigin;
    [HideInInspector] public Vector3 topRayOrigin;

    private void Start()
    {
        // Store default y scale
        startYscale = transform.localScale.y;
    }

    public void startMantle()
    {
        pMm.playerRigidBody.useGravity = false;
        pMm.playerRigidBody.drag = 0f;

        isMantling = true;

        Invoke(nameof(stopMantle), mantleLength);
    }

    public void handleMantle()
    {
        transform.position = new Vector3(transform.position.x, topMantleHit.point.y + .1f, transform.position.z);
        transform.localScale = new Vector3(transform.localScale.x, mantleYScale, transform.localScale.z);

        Vector3 flatMoveSpeed = pMm.moveDirection.normalized * pMm.moveSpeed;
        pMm.playerRigidBody.velocity = flatMoveSpeed;
    }

    public void stopMantle()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);

        pMm.playerRigidBody.useGravity = true;
        pMm.playerRigidBody.drag = pMm.groundedDrag;

        isMantling = false;
    }

    public bool canMantle()
    {
        playerSemiCircleRadius = pMm.bodyWidth / 2;

        forwardRayOrigin = new Vector3(transform.position.x, transform.position.y + playerSemiCircleRadius, transform.position.z);
        forwardRayDistance = (pMm.bodyWidth / 2) + maxMantleDistance;

        Debug.DrawRay(forwardRayOrigin, pMm.orientation.forward, color:Color.red ,forwardRayDistance);

        if (!Physics.Raycast(forwardRayOrigin, pMm.orientation.forward, out forwardMantleHit, forwardRayDistance, whatIsMantlable))
            return false;

        topRayOrigin = new Vector3(forwardMantleHit.point.x + (pMm.orientation.forward.x * .1f), 1f * maxMantleObjectHeight, forwardMantleHit.point.z + (pMm.orientation.forward.z * .1f));
        topRayDistance = maxMantleObjectHeight + .1f;

        if (!Physics.Raycast(topRayOrigin, Vector3.down, out topMantleHit, topRayDistance, whatIsMantlable))
            return false;

        return (topMantleHit.point.y - transform.position.y <= maxMantleObjectHeight) && pMm.verticalInput > 0f;
    }

}

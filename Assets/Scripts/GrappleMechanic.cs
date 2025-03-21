using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleMechanic : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Vector3 grapplePoint;
    private RaycastHit hit;
    private GameObject hitObject;
    private SpringJoint joint;
    private float distanceFromPoint;
    public LayerMask whatIsGrapplable;
    public Transform firePoint, camera, player;
    public float maxGrappleDistance = 100f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndGrapple();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void StartGrapple()
    {
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxGrappleDistance))
        {
            hitObject = hit.collider.gameObject;
            if ((whatIsGrapplable.value & (1 << hitObject.layer)) != 0)
            {
                grapplePoint = hit.point;

                joint = player.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;

                distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

                joint.maxDistance = distanceFromPoint * .08f;
                joint.minDistance = distanceFromPoint * .025f;

                joint.spring = 4.5f;
                joint.damper = 7f;
                joint.massScale = 4.5f;

                lineRenderer.positionCount = 2;
            }
        }
    }

    private void DrawRope()
    {
        if (!joint) return;

        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    private void EndGrapple()
    {
        lineRenderer.positionCount = 0;
        Destroy(joint);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 getGrapplingPoint()
    {
        return grapplePoint;
    }
}
